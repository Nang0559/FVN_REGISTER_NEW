using System.Data;
using System.Security.Cryptography;
using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/security/endpoints")]
[Authorize]
public sealed class EndpointInventoryController : ControllerBase
{
    private readonly FVNWEBAPPContext _db;
    private readonly EndpointInventoryService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppAuthorizationService _authorization;

    public EndpointInventoryController(
        FVNWEBAPPContext db,
        ICurrentUserService currentUser,
        AppAuthorizationService authorization)
    {
        _db = db;
        _service = new EndpointInventoryService(db);
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EndpointInventorySummaryDto>>>> GetAll(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var data = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EndpointInventorySummaryDto>>.Ok(data));
    }

    [HttpGet("{deviceKey}")]
    public async Task<ActionResult<ApiResponse<EndpointInventorySummaryDto>>> Get(string deviceKey, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var data = await _service.GetAsync(deviceKey, ct);
        return data == null
            ? NotFound(ApiResponse<EndpointInventorySummaryDto>.Fail("Không tìm thấy máy.", 404))
            : Ok(ApiResponse<EndpointInventorySummaryDto>.Ok(data));
    }

    // Machine-to-machine ingestion. The endpoint is anonymous at the ASP.NET
    // user-authentication layer, but every request must carry a valid per-device
    // credential. The server resolves DeviceKey from that credential and ignores
    // client-supplied EmployeeCode/EquipmentAssetId.
    [AllowAnonymous]
    [HttpPost("inventory")]
    public async Task<ActionResult<ApiResponse<EndpointInventorySummaryDto>>> Ingest(
        [FromBody] EndpointInventoryRequestDto request,
        CancellationToken ct)
    {
        var apiKey = Request.Headers["X-FVN-Device-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Unauthorized(ApiResponse<EndpointInventorySummaryDto>.Fail("Thiếu thông tin xác thực thiết bị.", 401));

        var deviceKey = await ResolveDeviceKeyAsync(apiKey, ct);
        if (string.IsNullOrWhiteSpace(deviceKey))
            return Unauthorized(ApiResponse<EndpointInventorySummaryDto>.Fail("Thông tin xác thực thiết bị không hợp lệ hoặc đã hết hạn.", 401));

        // Device identity and business ownership are server-side facts.
        // Never trust EmployeeCode/EquipmentAssetId supplied by an endpoint.
        var trustedRequest = request with
        {
            DeviceKey = deviceKey,
            EmployeeCode = null,
            EquipmentAssetId = null
        };

        try
        {
            var result = await _service.UpsertInventoryAsync(trustedRequest, ct);
            return Ok(ApiResponse<EndpointInventorySummaryDto>.Ok(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<EndpointInventorySummaryDto>.Fail(ex.Message));
        }
    }

    private async Task<string?> ResolveDeviceKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        byte[] hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        var connection = _db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) d.DeviceKey
            FROM dbo.F03EndpointCredentials c
            INNER JOIN dbo.F03EndpointDevices d ON d.Id = c.EndpointDeviceId
            WHERE c.SecretHash = @hash
              AND c.RevokedAtUtc IS NULL
              AND c.ExpiresAtUtc > SYSUTCDATETIME();
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@hash";
        parameter.DbType = DbType.Binary;
        parameter.Value = hash;
        command.Parameters.Add(parameter);

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value == null || value == DBNull.Value ? null : Convert.ToString(value);
    }
}

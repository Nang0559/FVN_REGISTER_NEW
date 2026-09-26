using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Infrastructure;
using FVN_REGISTER.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/security/endpoints")]
[Authorize]
public sealed class EndpointInventoryController : ControllerBase
{
    private readonly EndpointInventoryService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationService _authorization;

    public EndpointInventoryController(FVNWEBAPPContext db, ICurrentUserService currentUser, IAuthorizationService authorization)
    {
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
        return data == null ? NotFound(ApiResponse<EndpointInventorySummaryDto>.Fail("Không tìm thấy máy.", 404)) : Ok(ApiResponse<EndpointInventorySummaryDto>.Ok(data));
    }

    // Authenticated ingestion endpoint for initial rollout/testing. A dedicated
    // per-device credential endpoint must be used before broad Agent deployment.
    [HttpPost("inventory")]
    public async Task<ActionResult<ApiResponse<EndpointInventorySummaryDto>>> Ingest([FromBody] EndpointInventoryRequestDto request, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        try
        {
            var result = await _service.UpsertInventoryAsync(request, ct);
            return Ok(ApiResponse<EndpointInventorySummaryDto>.Ok(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<EndpointInventorySummaryDto>.Fail(ex.Message));
        }
    }
}

using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Security;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Application.Interfaces.Users;
using AppAuthorizationService = FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/security/endpoints")]
[Authorize]
public sealed class EndpointInventoryController : ControllerBase
{
    private readonly IEndpointInventoryService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly AppAuthorizationService _authorization;

    public EndpointInventoryController(IEndpointInventoryService service, ICurrentUserService currentUser, AppAuthorizationService authorization)
    {
        _service = service;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EndpointInventorySummaryDto>>>> GetAll(CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var data = await _service.GetAllAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<EndpointInventorySummaryDto>>.Success(data));
    }

    [HttpGet("{deviceKey}")]
    public async Task<ActionResult<ApiResponse<EndpointInventorySummaryDto>>> Get(string deviceKey, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentView, ct)) return Forbid();
        var data = await _service.GetAsync(deviceKey, ct);
        return data == null ? NotFound(ApiResponse<EndpointInventorySummaryDto>.Fail("Không tìm thấy máy.")) : Ok(ApiResponse<EndpointInventorySummaryDto>.Success(data));
    }

    // Authenticated ingestion endpoint for the initial rollout/testing phase.
    // A dedicated per-device credential endpoint must be introduced before deploying
    // the Windows Agent broadly; do not use a shared secret across all endpoints.
    [HttpPost("inventory")]
    public async Task<ActionResult<ApiResponse<EndpointInventorySummaryDto>>> Ingest([FromBody] EndpointInventoryRequestDto request, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();
        try
        {
            var result = await _service.UpsertInventoryAsync(request, ct);
            return Ok(ApiResponse<EndpointInventorySummaryDto>.Success(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<EndpointInventorySummaryDto>.Fail(ex.Message));
        }
    }
}

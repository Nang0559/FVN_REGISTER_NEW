using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Application.Interfaces.Users;
using FVN_REGISTER.Contract.Responses;
using FVN_REGISTER.Core.Constants;
using FVN_REGISTER.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/security/endpoints/credentials")]
[Authorize]
public sealed class EndpointCredentialController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService _authorization;
    private readonly EndpointCredentialService _service;

    public EndpointCredentialController(
        FVN_REGISTER.Infrastructure.FVNWEBAPPContext db,
        ICurrentUserService currentUser,
        FVN_REGISTER.Application.Interfaces.Security.IAuthorizationService authorization)
    {
        _currentUser = currentUser;
        _authorization = authorization;
        _service = new EndpointCredentialService(db);
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] EndpointCredentialProvisionDto request, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();

        try
        {
            var result = await _service.ProvisionAsync(request, user.UserId, ct);
            return Ok(ApiResponse<EndpointCredentialProvisionResult>.Ok(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<EndpointCredentialProvisionResult>.Fail(ex.Message));
        }
    }

    [HttpPost("{deviceKey}/revoke")]
    public async Task<IActionResult> Revoke(string deviceKey, CancellationToken ct)
    {
        var user = _currentUser.GetCurrentUser();
        if (user == null || !await _authorization.HasAsync(user, SecurityFunctionCodes.EquipmentImport, ct)) return Forbid();

        var changed = await _service.RevokeAsync(deviceKey, user.UserId, ct);
        return changed
            ? Ok(ApiResponse<object>.Ok(new { DeviceKey = deviceKey, Revoked = true }))
            : NotFound(ApiResponse<object>.Fail("Không tìm thấy credential đang hoạt động của thiết bị.", 404));
    }
}

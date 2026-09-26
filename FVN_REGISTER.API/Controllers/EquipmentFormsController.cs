using FVN_REGISTER.Application.Interfaces.Equipment;
using FVN_REGISTER.Contract.Dtos.EquipmentForms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FVN_REGISTER.API.Controllers;

[ApiController]
[Route("api/equipment/forms")]
[Authorize]
public sealed class EquipmentFormsController : ControllerBase
{
    private readonly IEquipmentFormService _service;

    public EquipmentFormsController(IEquipmentFormService service) => _service = service;

    [HttpGet("my/{assetId:int}")]
    public async Task<IActionResult> My(int assetId, CancellationToken ct)
        => ToResponse(await _service.GetMyApplicableFormsAsync(assetId, ct));

    [HttpGet("assignments/{equipmentSchemaKey}")]
    public async Task<IActionResult> Assignments(string equipmentSchemaKey, CancellationToken ct)
        => ToResponse(await _service.GetAssignmentsAsync(equipmentSchemaKey, ct));

    [HttpPost("assignments")]
    public async Task<IActionResult> Assign([FromBody] EquipmentFormAssignmentRequest request, CancellationToken ct)
        => ToResponse(await _service.AssignAsync(request, ct));

    [HttpPost("assignments/{id:int}/remove")]
    public async Task<IActionResult> RemoveAssignment(int id, CancellationToken ct)
        => ToResponse(await _service.RemoveAssignmentAsync(id, ct));

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] EquipmentFormSubmissionRequest request, CancellationToken ct)
        => ToResponse(await _service.SubmitAsync(request, ct));

    private static IActionResult ToResponse<T>(FVN_REGISTER.Contract.Utils.ServiceResult<T> result)
        => result.IsSuccess
            ? new OkObjectResult(FVN_REGISTER.Contract.Responses.ApiResponse<T>.Ok(result.Data!))
            : new BadRequestObjectResult(FVN_REGISTER.Contract.Responses.ApiResponse<T>.Fail(result.Message ?? "Không thể thực hiện thao tác."));
}

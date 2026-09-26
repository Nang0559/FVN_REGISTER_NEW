namespace FVN_REGISTER.Contract.Dtos.Authentication;

/// <summary>
/// Canonical authenticated-user identity contract shared by API, application and UI layers.
/// Keep this DTO free of infrastructure or persistence concerns.
/// </summary>
public sealed class UserIdentityDto
{
    public int UserId { get; init; }
    public string? EmployeeCode { get; init; }
    public string? DeptCode { get; init; }
    public string? PositionCode { get; init; }
    public bool IsLoggedIn { get; init; }
}

namespace FVN_REGISTER.Application.Interfaces.Security;

public sealed record EndpointSoftwareCatalogItemDto(
    long Id,
    string CatalogCode,
    string NormalizedName,
    string DisplayName,
    string? Publisher,
    string? VersionRule,
    string Status,
    string RiskLevel);

public sealed record EndpointSoftwareInstallRequestDto(
    long Id,
    long? EndpointDeviceId,
    string? EmployeeCode,
    long? CatalogId,
    string RequestedSoftwareName,
    string? RequestedPublisher,
    string? RequestedVersion,
    string Purpose,
    bool RequiresSecurityReview,
    string SecurityReviewStatus,
    long? ApprovalCaseId,
    string Status,
    long? AllowlistVersion);

public sealed record EndpointSoftwareChangeRequestDto(
    long Id,
    long? CatalogId,
    string Action,
    string ProposedSoftwareName,
    string? ProposedPublisher,
    string? ProposedVersionRule,
    string ScopeType,
    string? ScopeKey,
    string Reason,
    string SecurityReviewStatus,
    long? ApprovalCaseId,
    string Status);

public interface IEndpointGovernanceService
{
    Task<IReadOnlyList<EndpointSoftwareCatalogItemDto>> GetActiveSoftwareCatalogAsync(
        string? scopeKey,
        CancellationToken cancellationToken = default);

    Task<EndpointSoftwareInstallRequestDto> CreateInstallRequestAsync(
        long? endpointDeviceId,
        int? equipmentAssetId,
        string? employeeCode,
        long? catalogId,
        string softwareName,
        string? publisher,
        string? version,
        string purpose,
        int requesterUserId,
        CancellationToken cancellationToken = default);

    Task<EndpointSoftwareChangeRequestDto> CreateSoftwareChangeRequestAsync(
        long? catalogId,
        string action,
        string softwareName,
        string? publisher,
        string? versionRule,
        string scopeType,
        string? scopeKey,
        string reason,
        int requesterUserId,
        CancellationToken cancellationToken = default);

    Task ReviewSoftwareChangeAsync(
        long requestId,
        bool approved,
        string? note,
        int reviewerUserId,
        CancellationToken cancellationToken = default);
}

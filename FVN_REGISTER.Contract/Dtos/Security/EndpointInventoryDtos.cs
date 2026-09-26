namespace FVN_REGISTER.Contract.Dtos.Security;

public sealed record EndpointSoftwareInventoryDto(
    string Name,
    string? DisplayName,
    string? Publisher,
    string? Version,
    string? Architecture,
    DateTime? InstallDate,
    string? InstallLocation);

public sealed record EndpointServiceInventoryDto(
    string ServiceName,
    string? DisplayName,
    string? State,
    string? StartMode,
    string? BinaryPathHash);

public sealed record EndpointInventoryRequestDto(
    string DeviceKey,
    string? ComputerName,
    string? SerialNumber,
    string? OsName,
    string? OsVersion,
    string? EmployeeCode,
    int? EquipmentAssetId,
    string? AgentVersion,
    IReadOnlyList<EndpointSoftwareInventoryDto> Software,
    IReadOnlyList<EndpointServiceInventoryDto> Services);

public sealed record EndpointInventorySummaryDto(
    long EndpointId,
    string DeviceKey,
    string? ComputerName,
    string? SerialNumber,
    string Status,
    DateTime? LastSeenUtc,
    int SoftwareCount,
    int ServiceCount);

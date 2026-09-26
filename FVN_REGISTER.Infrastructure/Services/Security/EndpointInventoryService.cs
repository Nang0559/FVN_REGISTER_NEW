using FVN_REGISTER.Application.Interfaces.Security;
using FVN_REGISTER.Contract.Dtos.Security;
using Microsoft.EntityFrameworkCore;

namespace FVN_REGISTER.Infrastructure.Services.Security;

public sealed class EndpointInventoryService : IEndpointInventoryService
{
    private readonly FVNWEBAPPContext _db;

    public EndpointInventoryService(FVNWEBAPPContext db) => _db = db;

    public async Task<EndpointInventorySummaryDto> UpsertInventoryAsync(EndpointInventoryRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceKey))
            throw new ArgumentException("DeviceKey is required.", nameof(request));
        if (request.DeviceKey.Length > 100)
            throw new ArgumentException("DeviceKey is too long.", nameof(request));
        if (request.Software.Count > 5000 || request.Services.Count > 2000)
            throw new ArgumentException("Inventory payload exceeds the supported limit.", nameof(request));

        var deviceKey = request.DeviceKey.Trim();
        var device = await _db.EndpointDevices.SingleOrDefaultAsync(x => x.DeviceKey == deviceKey, cancellationToken);
        if (device == null)
        {
            device = new F03EndpointDevice { DeviceKey = deviceKey, CreatedAt = DateTime.UtcNow };
            _db.EndpointDevices.Add(device);
        }

        device.ComputerName = Trim(request.ComputerName, 255);
        device.SerialNumber = Trim(request.SerialNumber, 255);
        device.OsName = Trim(request.OsName, 255);
        device.OsVersion = Trim(request.OsVersion, 100);
        device.EmployeeCode = Trim(request.EmployeeCode, 50);
        device.AgentVersion = Trim(request.AgentVersion, 50);
        device.LastSeenUtc = DateTime.UtcNow;
        device.Status = "Online";
        device.Source = "FVNAgent";
        if (request.EquipmentAssetId.HasValue)
        {
            var assetExists = await _db.EquipmentAssets.AsNoTracking().AnyAsync(x => x.Id == request.EquipmentAssetId.Value, cancellationToken);
            if (assetExists) device.EquipmentAssetId = request.EquipmentAssetId;
        }
        device.UpdatedAt = DateTime.UtcNow;

        // A complete snapshot is authoritative for the current state. Replacing the
        // child rows prevents stale software/service records from surviving removal.
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var oldSoftware = await _db.EndpointSoftwareInventory.Where(x => x.EndpointDeviceId == device.Id).ToListAsync(cancellationToken);
        var oldServices = await _db.EndpointServiceInventory.Where(x => x.EndpointDeviceId == device.Id).ToListAsync(cancellationToken);
        _db.EndpointSoftwareInventory.RemoveRange(oldSoftware);
        _db.EndpointServiceInventory.RemoveRange(oldServices);

        var detectedAt = DateTime.UtcNow;
        foreach (var item in request.Software.Where(x => !string.IsNullOrWhiteSpace(x.Name)).GroupBy(x => Normalize(x.Name), StringComparer.OrdinalIgnoreCase).Select(x => x.First()))
        {
            _db.EndpointSoftwareInventory.Add(new F03EndpointSoftwareInventory
            {
                EndpointDeviceId = device.Id,
                NormalizedName = Normalize(item.Name),
                DisplayName = Trim(item.DisplayName ?? item.Name, 255),
                Publisher = Trim(item.Publisher, 255),
                Version = Trim(item.Version, 100),
                Architecture = Trim(item.Architecture, 30),
                InstallDate = item.InstallDate,
                InstallLocation = Trim(item.InstallLocation, 1000),
                DetectedAtUtc = detectedAt,
                Source = "FVNAgent"
            });
        }

        foreach (var item in request.Services.Where(x => !string.IsNullOrWhiteSpace(x.ServiceName)).GroupBy(x => x.ServiceName.Trim(), StringComparer.OrdinalIgnoreCase).Select(x => x.First()))
        {
            _db.EndpointServiceInventory.Add(new F03EndpointServiceInventory
            {
                EndpointDeviceId = device.Id,
                ServiceName = Trim(item.ServiceName, 255)!,
                DisplayName = Trim(item.DisplayName, 255),
                State = Trim(item.State, 30),
                StartMode = Trim(item.StartMode, 30),
                BinaryPathHash = Trim(item.BinaryPathHash, 128),
                DetectedAtUtc = detectedAt,
                Source = "FVNAgent"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return await ToSummaryAsync(device.Id, cancellationToken);
    }

    public async Task<EndpointInventorySummaryDto?> GetAsync(string deviceKey, CancellationToken cancellationToken = default)
    {
        var device = await _db.EndpointDevices.AsNoTracking().SingleOrDefaultAsync(x => x.DeviceKey == deviceKey.Trim(), cancellationToken);
        return device == null ? null : await ToSummaryAsync(device.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<EndpointInventorySummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _db.EndpointDevices.AsNoTracking().OrderBy(x => x.ComputerName).ThenBy(x => x.DeviceKey).ToListAsync(cancellationToken);
        var ids = devices.Select(x => x.Id).ToArray();
        var software = await _db.EndpointSoftwareInventory.AsNoTracking().Where(x => ids.Contains(x.EndpointDeviceId)).GroupBy(x => x.EndpointDeviceId).Select(x => new { Id = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Id, cancellationToken);
        var services = await _db.EndpointServiceInventory.AsNoTracking().Where(x => ids.Contains(x.EndpointDeviceId)).GroupBy(x => x.EndpointDeviceId).Select(x => new { Id = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Id, cancellationToken);
        return devices.Select(x => new EndpointInventorySummaryDto(x.Id, x.DeviceKey, x.ComputerName, x.SerialNumber, x.Status, x.LastSeenUtc, software.GetValueOrDefault(x.Id)?.Count ?? 0, services.GetValueOrDefault(x.Id)?.Count ?? 0)).ToArray();
    }

    private async Task<EndpointInventorySummaryDto> ToSummaryAsync(long id, CancellationToken cancellationToken)
    {
        var device = await _db.EndpointDevices.AsNoTracking().SingleAsync(x => x.Id == id, cancellationToken);
        var softwareCount = await _db.EndpointSoftwareInventory.AsNoTracking().CountAsync(x => x.EndpointDeviceId == id, cancellationToken);
        var serviceCount = await _db.EndpointServiceInventory.AsNoTracking().CountAsync(x => x.EndpointDeviceId == id, cancellationToken);
        return new EndpointInventorySummaryDto(device.Id, device.DeviceKey, device.ComputerName, device.SerialNumber, device.Status, device.LastSeenUtc, softwareCount, serviceCount);
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? Trim(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

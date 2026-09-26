using System.Net.Http.Json;
using FVN_REGISTER.Contract.Dtos.Security;
using Microsoft.Extensions.Options;

namespace FVN_REGISTER.EndpointAgent;

public sealed class EndpointWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointWorker> _logger;
    private readonly EndpointAgentOptions _options;
    private readonly EndpointCollector _collector = new();

    public EndpointWorker(IHttpClientFactory httpClientFactory, IOptions<EndpointAgentOptions> options, ILogger<EndpointWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.DeviceKey) || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("Endpoint Agent is not configured: DeviceKey and ApiKey are required.");
            return;
        }

        var delay = TimeSpan.FromMinutes(Math.Clamp(_options.IntervalMinutes, 5, 1440));
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SendInventoryAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Endpoint inventory synchronization failed."); }
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task SendInventoryAsync(CancellationToken cancellationToken)
    {
        var request = new EndpointInventoryRequestDto(
            _options.DeviceKey.Trim(),
            Environment.MachineName,
            GetSerialNumber(),
            OperatingSystem.IsWindows() ? "Windows" : OperatingSystem.OSDescription,
            Environment.OSVersion.VersionString,
            null,
            null,
            typeof(EndpointWorker).Assembly.GetName().Version?.ToString(),
            _collector.CollectSoftware(),
            _collector.CollectServices());

        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-FVN-Device-Key");
        client.DefaultRequestHeaders.Remove("X-FVN-Device-Api-Key");
        client.DefaultRequestHeaders.Add("X-FVN-Device-Key", request.DeviceKey);
        client.DefaultRequestHeaders.Add("X-FVN-Device-Api-Key", _options.ApiKey);

        using var response = await client.PostAsJsonAsync("api/security/endpoints/inventory", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        _logger.LogInformation("Endpoint inventory synchronized. Software={SoftwareCount}, Services={ServiceCount}.", request.Software.Count, request.Services.Count);
    }

    private static string? GetSerialNumber()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\BIOS");
            return key?.GetValue("SystemSerialNumber")?.ToString();
        }
        catch { return null; }
    }
}

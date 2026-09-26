using FVN_REGISTER.EndpointAgent;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => options.ServiceName = "FVN Register Endpoint Agent");
builder.Services.AddHttpClient();
builder.Services.AddHostedService<EndpointWorker>();

await builder.Build().RunAsync();

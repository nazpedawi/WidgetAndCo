using ApplicationServices.Extensions;
using Azure.Monitor.OpenTelemetry.Exporter;
using DataLayer.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

string Setting(string name) => builder.Configuration[name]
    ?? throw new InvalidOperationException($"Setting '{name}' is missing in local.settings.json.");

// Same business tier + data tier as the WebApi
builder.Services.AddApplicationServices();
builder.Services.AddCosmosDb(Setting("CosmosDb"));
builder.Services.AddSqlDb(Setting("Sql"));      
builder.Services.AddAzureStorage(Setting("AzureStorage"));

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var host = builder.Build();
await host.Services.InitializeCosmosAsync();
host.Run();
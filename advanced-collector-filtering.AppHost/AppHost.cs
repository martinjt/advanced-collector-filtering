using advanced_collector_filtering.AppHost.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var collector = builder.AddOpenTelemetryCollector("collector", "./config.yaml")
    .WithAppForwarding();

#pragma warning disable ASPIRECOSMOSDB001
var cosmosdb = builder.AddAzureCosmosDB("weather")
    .RunAsPreviewEmulator(emulator =>
    {
        emulator.WithDataExplorer(10346);
    });
var weatherdb = cosmosdb.AddCosmosDatabase("weatherdb");
var container = weatherdb
    .AddContainer("locations", "/country")
    .SeedDatabase();

var apiService = builder.AddProject<Projects.advanced_collector_filtering_ApiService>("apiservice")
    .WithReference(cosmosdb)
    .WaitFor(container)
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("https", url =>
    {
        url.Url = "/scalar/v1";
        url.DisplayText = "API Browser";
    });


await builder.Build().RunAsync();

using advanced_collector_filtering.ApiService.Models;
using advanced_collector_filtering.ApiService.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Azure Cosmos DB
builder.AddAzureCosmosClient("weather", settings =>
{
    settings.DisableTracing = false;
});

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Servers = [];
    });
}

// Get weather by location (point read)
app.MapGet("/weather/{country}/{location}", async (string country, string location, IWeatherService weatherService) =>
{
    var weather = await weatherService.GetWeatherByLocationAsync(country, location);
    return weather is not null ? Results.Ok(weather) : Results.NotFound($"Weather data for '{location}' not found");
})
.WithName("GetWeatherByLocation")
.WithTags("Weather");


app.MapGet("/weather/{country}", async (string country, IWeatherService weatherService) =>
{
    var locations = await weatherService.GetWeatherLocationsByCountry(country);
    return Results.Ok(locations);
})
.WithName("GetWeatherByCountry")
.WithTags("Weather");

// Get all available locations
app.MapGet("/weather/", async (IWeatherService weatherService) =>
{
    var locations = await weatherService.GetAllLocationsAsync();
    return Results.Ok(locations.Select(l => new { l.Location, l.Country, l.Latitude, l.Longitude }));
})
.WithName("GetAllLocations")
.WithTags("Weather");

// Legacy endpoint for backwards compatibility
app.MapGet("/weatherforecast", async (IWeatherService weatherService) =>
{
    var locations = await weatherService.GetAllLocationsAsync();
    var forecast = locations.FirstOrDefault()?.Forecast ?? new List<WeatherData>();
    return forecast.Select(w => new WeatherForecast(w.Date, w.TemperatureC, w.Summary)).ToArray();
})
.WithName("GetWeatherForecast")
.WithTags("Weather");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

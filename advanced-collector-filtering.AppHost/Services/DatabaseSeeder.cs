using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using advanced_collector_filtering.ApiService.Models;
using Aspire.Hosting.Azure;
using Microsoft.Extensions.DependencyInjection;

namespace advanced_collector_filtering.AppHost.Services;

public static class DatabaseSeeder
{
    public static IResourceBuilder<AzureCosmosDBContainerResource> SeedDatabase(this IResourceBuilder<AzureCosmosDBContainerResource> container)
    {
        container.OnResourceReady(async (resource, readyEvent, cancellationToken) =>
        {
            await Task.Delay(1000);
            var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
            var cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                LimitToEndpoint = true
            });
            var logger = readyEvent.Services.GetRequiredService<ILogger<DistributedApplication>>();
            await SeedWeatherDataAsync(cosmosClient,
                container.Resource.Parent.DatabaseName,
                container.Resource.ContainerName,
                logger);
        });
        return container;
    }

    public static async Task SeedWeatherDataAsync(CosmosClient cosmosClient, string databaseName, string containerName, ILogger<DistributedApplication> logger)
    {
        try
        {
            logger.LogInformation("Starting database seeding process");

            var container = cosmosClient.GetContainer(databaseName, containerName);
            var locations = GetSeedLocations();

            foreach (var location in locations)
            {
                try
                {
                    // Item doesn't exist, create it
                    await container.CreateItemAsync(location, new PartitionKey(location.Id));
                    logger.LogInformation("Created weather data for {Location}", location.Location);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to seed weather data for {Location}", location.Location);
                }
            }

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogInformation("ConnectionString: {ConnectionString}", cosmosClient.Endpoint);
            logger.LogError(ex, "Failed to complete database seeding");
            throw;
        }
    }

    private static List<WeatherLocation> GetSeedLocations()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var locations = new List<(string Name, string Country, double Lat, double Lon)>
        {
            ("London", "United Kingdom", 51.5074, -0.1278),
            ("New York", "United States", 40.7128, -74.0060),
            ("Tokyo", "Japan", 35.6762, 139.6503),
            ("Sydney", "Australia", -33.8688, 151.2093),
            ("Paris", "France", 48.8566, 2.3522),
            ("Berlin", "Germany", 52.5200, 13.4050),
            ("Toronto", "Canada", 43.6532, -79.3832),
            ("Mumbai", "India", 19.0760, 72.8777),
            ("São Paulo", "Brazil", -23.5558, -46.6396),
            ("Cairo", "Egypt", 30.0444, 31.2357)
        };

        return [.. locations.Select(loc => new WeatherLocation
        {
            Id = loc.Name.ToLowerInvariant(),
            Location = loc.Name.ToLowerInvariant(),
            Country = loc.Country,
            Latitude = loc.Lat,
            Longitude = loc.Lon,
            CurrentWeather = GenerateWeatherData(DateOnly.FromDateTime(DateTime.Now)),
            Forecast = [.. Enumerable.Range(1, 5).Select(i => GenerateWeatherData(DateOnly.FromDateTime(DateTime.Now.AddDays(i))))]
        })];
    }

    private static WeatherData GenerateWeatherData(DateOnly date)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        return new WeatherData
        {
            Date = date,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)],
            Humidity = Random.Shared.Next(30, 90),
            WindSpeed = Random.Shared.NextDouble() * 20
        };
    }
}

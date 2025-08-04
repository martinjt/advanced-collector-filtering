using Microsoft.Azure.Cosmos;
using advanced_collector_filtering.ApiService.Models;
using System.Net;

namespace advanced_collector_filtering.ApiService.Services;

public interface IWeatherService
{
    Task<WeatherLocation?> GetWeatherByLocationAsync(string country, string location);
    Task<IEnumerable<WeatherLocation>> GetWeatherLocationsByCountry(string country);
    Task<IEnumerable<WeatherLocation>> GetAllLocationsAsync();
}

public class WeatherService : IWeatherService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(CosmosClient cosmosClient, ILogger<WeatherService> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _container = _cosmosClient.GetContainer("weatherdb", "locations");
    }

    public async Task<WeatherLocation?> GetWeatherByLocationAsync(string country, string location)
    {
        try
        {
            // Point read using the location as both id and partition key
            var locationKey = location.ToLowerInvariant();
            var response = await _container.ReadItemAsync<WeatherLocation>(
                locationKey,
                new PartitionKey(locationKey)
            );
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Location {Location} not found", location);
            return null;
        }
    }

    public async Task<IEnumerable<WeatherLocation>> GetWeatherLocationsByCountry(string country)
    {
        // partition query
        var query = new QueryDefinition("SELECT * FROM c WHERE c.country = @country")
            .WithParameter("@country", country);
        var iterator = _container.GetItemQueryIterator<WeatherLocation>(query, requestOptions: new QueryRequestOptions
        {
            PartitionKey = new PartitionKey(country)
        });
        var results = new List<WeatherLocation>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IEnumerable<WeatherLocation>> GetAllLocationsAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<WeatherLocation>(query);
        var results = new List<WeatherLocation>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }
}

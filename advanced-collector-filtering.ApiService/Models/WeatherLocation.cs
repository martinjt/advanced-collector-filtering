namespace advanced_collector_filtering.ApiService.Models;

public class WeatherLocation
{
    public required string Id { get; set; }

    public required string Location { get; set; }

    public required string Country { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public WeatherData? CurrentWeather { get; set; }

    public List<WeatherData> Forecast { get; set; } = new();
}

public class WeatherData
{
    public DateOnly Date { get; set; }
    
    public int TemperatureC { get; set; }
    
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    
    public required string Summary { get; set; }
    
    public int Humidity { get; set; }

    public double WindSpeed { get; set; }
}

# .NET Aspire Starter with CosmosDB

A basic .NET Aspire starter application demonstrating CosmosDB integration with automatic database seeding and tracing.

## What's Different

This extends the standard Aspire starter template with:

- **CosmosDB Integration**: Uses Azure CosmosDB emulator with automatic provisioning
- **Database Seeding**: Automatic seeding from the AppHost using Aspire resource lifecycle events
- **Enhanced Tracing**: CosmosDB operations are traced and visible in the Aspire dashboard

## Quick Start

```bash
dotnet run --project advanced-collector-filtering.AppHost
```

Access the Aspire dashboard at `https://localhost:17183` to see:

- CosmosDB traces and metrics
- Automatic database seeding logs
- API service health and performance

## CosmosDB Features

### Database Seeding

The AppHost automatically seeds the CosmosDB with weather data for 10 cities using Aspire resource lifecycle events:

### Tracing Integration

CosmosDB operations are automatically traced and visible in the Aspire dashboard:

## API Endpoints

There is a mix of endpoints that allow for Point Reads, Partition Queries, and full database reads.

- `GET /weather/{country}/{location}` - Get weather for specific location
- `GET /weather/{country}` - Get all locations in a country
- `GET /weather/` - Get all locations
- `GET /weatherforecast` - Legacy endpoint

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static Helpers.Tools;

namespace Helpers;

internal static class LocationServices
{
    private static readonly HttpClient s_httpClient = new();
    private static readonly ConcurrentDictionary<string, double> s_routeCache = new();

    static LocationServices()
    {
        s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetClient/1.0");
    }

    internal static async Task<Location?> GetLocationOfAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";
        var results = await s_httpClient.GetFromJsonAsync<List<NominatimResult>>(url);

        if (results?.FirstOrDefault() is not { } first) return null;

        return new Location
        {
            Latitude = double.TryParse(first.Lat, out var lat) ? lat : (double?)null,
            Longitude = double.TryParse(first.Lon, out var lon) ? lon : (double?)null
        };
    }

    internal static async Task<double> GetRouteDistanceKmAsync(
        double? fromLat, double? fromLon, double toLat, double toLon, string profile = "driving")
    {
        string key = $"{profile}:{fromLat:F4},{fromLon:F4}->{toLat:F4},{toLon:F4}";
        if (s_routeCache.TryGetValue(key, out var cached)) return cached;

        var url = $"https://router.project-osrm.org/route/v1/{profile}/{fromLon},{fromLat};{toLon},{toLat}?overview=false";
        var response = await s_httpClient.GetFromJsonAsync<OsrmResponse>(url);

        var distanceKm = (response?.Routes?.FirstOrDefault()?.Distance ?? double.PositiveInfinity) / 1000.0;

        if (distanceKm < double.PositiveInfinity)
            s_routeCache[key] = distanceKm;

        return distanceKm;
    }
}

// Simple DTO for coordinates used by this helper
internal sealed class Location
{
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
}

// Minimal DTOs to make GetFromJsonAsync work
public record NominatimResult(string Lat, string Lon);
public record OsrmResponse(List<OsrmRoute> Routes);
public record OsrmRoute(double Distance);

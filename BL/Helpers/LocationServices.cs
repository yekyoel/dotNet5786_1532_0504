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
    private static readonly SemaphoreSlim s_throttle = new(2, 2);

    // TODO: Replace with your actual LocationIQ API Key
    private const string LocationIqKey = "6967dfd261d30293942658ejgca76f1";

    static LocationServices()
    {
        s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetClient/1.0");
    }

    internal static async Task<Location?> GetLocationOfAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;

        // Using LocationIQ Geocoding API
        var url = $"https://us1.locationiq.com/v1/search.php?key={LocationIqKey}&q={Uri.EscapeDataString(address)}&format=json";

        try
        {
            var results = await s_httpClient.GetFromJsonAsync<List<NominatimResult>>(url);

            if (results?.FirstOrDefault() is not { } first) return null;

            return new Location
            {
                Latitude = double.TryParse(first.Lat, out var lat) ? lat : (double?)null,
                Longitude = double.TryParse(first.Lon, out var lon) ? lon : (double?)null
            };
        }
        catch
        {
            // If LocationIQ fails (e.g. invalid key, limit reached), fallback or return null
            return null;
        }
    }

    internal static async Task<double> GetRouteDistanceKmAsync(
        double? fromLat, double? fromLon, double toLat, double toLon, string profile = "driving")
    {
        if (fromLat == null || fromLon == null) return double.PositiveInfinity;

        // 1. Check Cache
        string key = $"{profile}:{fromLat:F4},{fromLon:F4}->{toLat:F4},{toLon:F4}";
        if (s_routeCache.TryGetValue(key, out var cached)) return cached;

        // 2. LocationIQ Directions API URL
        // Note: LocationIQ uses {longitude},{latitude} format just like OSRM
        var url = $"https://us1.locationiq.com/v1/directions/{profile}/{fromLon},{fromLat};{toLon},{toLat}?key={LocationIqKey}&overview=false";

        try
        {
            var response = await s_httpClient.GetFromJsonAsync<OsrmResponse>(url);

            // LocationIQ returns the same JSON structure as OSRM
            var distanceKm = (response?.Routes?.FirstOrDefault()?.Distance ?? double.PositiveInfinity) / 1000.0;

            if (distanceKm < double.PositiveInfinity)
                s_routeCache[key] = distanceKm;

            return distanceKm;
        }
        catch
        {
            // If API fails (limit reached, etc.), return infinity to trigger your aerial fallback
            return double.PositiveInfinity;
        }
        finally
        {
            // Always release the gate so the next request can go through
            s_throttle.Release();
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
}

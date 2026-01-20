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


    private const string LocationIqKey = "6967dfd261d30293942658ejgca76f1";

    static LocationServices()
    {
        s_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DotNetClient/1.0");
    }


    /// <summary>
    /// Asynchronously calculates the estimated route distance, in kilometers, between two geographic coordinates using
    /// the specified travel profile.
    /// </summary>
    /// <remarks>The method uses a cached value if available and otherwise queries the LocationIQ Directions
    /// API. If the API call fails or required origin coordinates are not provided, the method returns <see
    /// cref="double.PositiveInfinity"/> to indicate that a valid route distance could not be determined.</remarks>
    /// <param name="fromLat">The latitude of the starting location. If <paramref name="fromLat"/> is <see langword="null"/>, the method
    /// returns <see cref="double.PositiveInfinity"/>.</param>
    /// <param name="fromLon">The longitude of the starting location. If <paramref name="fromLon"/> is <see langword="null"/>, the method
    /// returns <see cref="double.PositiveInfinity"/>.</param>
    /// <param name="toLat">The latitude of the destination location.</param>
    /// <param name="toLon">The longitude of the destination location.</param>
    /// <param name="profile">The travel profile to use for route calculation, such as "driving" or "walking". Defaults to "driving" if not
    /// specified.</param>
    /// <returns>A <see cref="double"/> value representing the route distance in kilometers. Returns <see
    /// cref="double.PositiveInfinity"/> if the route cannot be calculated.</returns>
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


    /// <summary>
    /// Represents the response from an OSRM routing query, containing one or more calculated routes.
    /// </summary>
    /// <param name="Routes">A list of <see cref="OsrmRoute"/> objects representing the possible routes returned by the OSRM service. Cannot
    /// be null.</param>
    public record OsrmResponse(List<OsrmRoute> Routes);

    /// <summary>
    /// Represents a route calculated by OSRM, including the total distance of the route.
    /// </summary>
    /// <param name="Distance">The total length of the route, in meters.</param>
    public record OsrmRoute(double Distance);
}

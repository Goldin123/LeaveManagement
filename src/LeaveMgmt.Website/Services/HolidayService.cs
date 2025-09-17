using System.Net.Http.Json;
using LeaveMgmt.Website.Models;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Provides holiday data to the Blazor website.
/// Calls the API via IHttpClientFactory and falls back
/// to built-in defaults if the API is unavailable.
/// </summary>
public sealed class HolidayService : IHolidayService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<HolidayService> _logger;

    public HolidayService(IHttpClientFactory httpFactory, ILogger<HolidayService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    /// <summary>
    /// Builds an HttpClient configured for the API,
    /// attaching the JWT token if available.
    /// </summary>
    private HttpClient Client()
    {
        var c = _httpFactory.CreateClient("api");

        if (!string.IsNullOrWhiteSpace(LoggedUser.Token))
        {
            c.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", LoggedUser.Token);
        }

        return c;
    }

    // Fallback list in case the API is unavailable
    private static readonly List<Holiday> _fallback = new()
    {
        new Holiday { Name = "New Year's Day", Date = new DateTime(2025, 1, 1) },
        new Holiday { Name = "Christmas Day", Date = new DateTime(2025, 12, 25) }
    };

    /// <inheritdoc />
    public async Task<List<Holiday>> GetAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching holidays for {Year}", year);

            var c = Client();
            var data = await c.GetFromJsonAsync<List<Holiday>>($"api/holidays/{year}");

            if (data is null || data.Count == 0)
            {
                _logger.LogWarning("No holidays returned by API for {Year}, falling back to defaults", year);
                return _fallback;
            }

            _logger.LogInformation("Successfully fetched {Count} holidays for {Year}", data.Count, year);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch holidays for {Year}, falling back to defaults", year);
            return _fallback;
        }
    }
}

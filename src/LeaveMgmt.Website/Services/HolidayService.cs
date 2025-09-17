// File: LeaveMgmt.Website/Services/HolidayService.cs
using System.Net.Http.Json;
using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

public sealed class HolidayService
{
    private readonly IHttpClientFactory _httpFactory;

    public HolidayService(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

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

    // fallback list in case API is unavailable
    private static readonly List<Holiday> _fallback = new()
    {
        new Holiday { Name = "New Year's Day", Date = new DateTime(2025, 1, 1) },
        new Holiday { Name = "Christmas Day", Date = new DateTime(2025, 12, 25) }
    };

    public async Task<List<Holiday>> GetAsync(int year)
    {
        try
        {
            var c = Client();
            var data = await c.GetFromJsonAsync<List<Holiday>>($"api/holidays/{year}");
            return data ?? _fallback;
        }
        catch
        {
            return _fallback;
        }
    }
}

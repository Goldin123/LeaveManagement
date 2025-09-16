using System.Net.Http.Json;
using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

public sealed class LeaveTypeService
{
    private readonly IHttpClientFactory _httpFactory;

    public LeaveTypeService(IHttpClientFactory httpFactory)
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
    private static readonly List<LeaveType> _fallback = new()
    {
        new LeaveType { Id = Guid.Parse("C34DE22D-DBEA-40AC-84C4-F1B5A4F335C3"), Name = "Annual" },
        new LeaveType { Id = Guid.Parse("822DFD3C-FF3A-458A-85EA-1BA32F5DB32D"), Name = "Sick" },
        new LeaveType { Id = Guid.Parse("4731EE57-627D-4639-B1DE-10E47DF45DC6"), Name = "Unpaid" }
    };

    public async Task<List<LeaveType>> GetAsync()
    {
        try
        {
            var c = Client();
            var data = await c.GetFromJsonAsync<List<LeaveType>>("api/leave-types");
            return data ?? _fallback;
        }
        catch
        {
            return _fallback;
        }
    }
}

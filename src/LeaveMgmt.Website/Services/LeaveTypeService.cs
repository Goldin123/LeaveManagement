using System.Net.Http.Json;
using LeaveMgmt.Website.Models;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Provides leave type data to the Blazor website.
/// Calls the API via IHttpClientFactory and falls back
/// to built-in defaults if the API is unavailable.
/// </summary>
public sealed class LeaveTypeService : ILeaveTypeService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<LeaveTypeService> _logger;

    public LeaveTypeService(IHttpClientFactory httpFactory, ILogger<LeaveTypeService> logger)
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
    private static readonly List<LeaveType> _fallback = new()
    {
        new LeaveType { Id = Guid.Parse("C34DE22D-DBEA-40AC-84C4-F1B5A4F335C3"), Name = "Annual" },
        new LeaveType { Id = Guid.Parse("822DFD3C-FF3A-458A-85EA-1BA32F5DB32D"), Name = "Sick" },
        new LeaveType { Id = Guid.Parse("4731EE57-627D-4639-B1DE-10E47DF45DC6"), Name = "Unpaid" }
    };

    /// <inheritdoc />
    public async Task<List<LeaveType>> GetAsync()
    {
        try
        {
            _logger.LogInformation("Fetching leave types from API");

            var c = Client();
            var data = await c.GetFromJsonAsync<List<LeaveType>>("api/leave-types");

            if (data is null || data.Count == 0)
            {
                _logger.LogWarning("API returned no leave types, falling back to defaults");
                return _fallback;
            }

            _logger.LogInformation("Successfully fetched {Count} leave types", data.Count);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch leave types, falling back to defaults");
            return _fallback;
        }
    }
}

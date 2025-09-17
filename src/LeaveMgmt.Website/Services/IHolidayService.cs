using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Defines holiday operations for the website layer.
/// </summary>
public interface IHolidayService
{
    /// <summary>
    /// Retrieves holidays for the given year.
    /// Falls back to defaults if the API call fails.
    /// </summary>
    Task<List<Holiday>> GetAsync(int year);
}

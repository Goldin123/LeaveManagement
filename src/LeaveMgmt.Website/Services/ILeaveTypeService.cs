using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Defines leave type operations for the website layer.
/// </summary>
public interface ILeaveTypeService
{
    /// <summary>
    /// Retrieves all available leave types.
    /// Falls back to defaults if the API call fails.
    /// </summary>
    Task<List<LeaveType>> GetAsync();
}

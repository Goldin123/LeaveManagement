using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Defines leave request operations for the website layer.
/// </summary>
public interface ILeaveRequestService
{
    Task<ApiResult<Guid>> SubmitAsync(SubmitLeaveRequest dto);
    Task<List<LeaveRequestListItem>> GetByEmployeeAsync(Guid employeeId);
    Task<List<LeaveRequestListItem>> GetByEmployeeAsync(); // overload using JWT
    Task<ApiResult<bool>> ApproveAsync(ApproveRequest dto);
    Task<ApiResult<bool>> RejectAsync(RejectRequest dto);
    Task<ApiResult<bool>> RetractAsync(RetractRequest dto);
    Task<List<LeaveRequestListItem>> GetAllAsync();
    Task<List<UserDto>> GetAllUsersAsync();
}

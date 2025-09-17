using System.Net.Http.Json;
using LeaveMgmt.Website.Models;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Provides leave request operations for the Blazor website.
/// Communicates with the API and enriches results with leave types and holidays.
/// </summary>
public sealed class LeaveRequestService : ILeaveRequestService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILeaveTypeService _leaveTypes;
    private readonly IHolidayService _holiday;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        IHttpClientFactory httpFactory,
        ILeaveTypeService leaveTypes,
        IHolidayService holiday,
        ILogger<LeaveRequestService> logger)
    {
        _httpFactory = httpFactory;
        _leaveTypes = leaveTypes;
        _holiday = holiday;
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

    /// <inheritdoc />
    public async Task<ApiResult<Guid>> SubmitAsync(SubmitLeaveRequest dto)
    {
        _logger.LogInformation("Submitting leave request for employee via API");

        var c = Client();
        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee identity from JWT");
            return ApiResult<Guid>.Fail("Invalid employee identity.");
        }

        dto.EmployeeId = employeeId;

        var resp = await c.PostAsJsonAsync("api/leave-requests", dto);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to submit leave request: {Error}", err);
            return ApiResult<Guid>.Fail(err);
        }

        var result = await resp.Content.ReadFromJsonAsync<SubmitLeaveResponse>();
        if (result is null)
        {
            _logger.LogWarning("Empty response received after submitting leave request");
            return ApiResult<Guid>.Fail("Empty response");
        }

        _logger.LogInformation("Leave request {LeaveRequestId} submitted successfully", result.Id);
        return ApiResult<Guid>.Ok(result.Id);
    }

    /// <inheritdoc />
    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync(Guid employeeId)
    {
        _logger.LogInformation("Fetching leave requests for employee {EmployeeId}", employeeId);

        var c = Client();
        var url = $"api/leave-requests/by-employee/{employeeId}";
        var data = await c.GetFromJsonAsync<List<LeaveRequestListItem>>(url) ?? new();

        var types = await _leaveTypes.GetAsync();

        foreach (var req in data)
        {
            var lt = types.FirstOrDefault(t => t.Id == req.LeaveTypeId);
            if (lt is not null)
            {
                req.LeaveTypeName = lt.Name;
                req.Days = await Helpers.Helpers.CalculateWorkingDaysAsync(req.StartDate, req.EndDate, _holiday);
            }
        }

        _logger.LogInformation("Fetched {Count} leave requests for employee {EmployeeId}", data.Count, employeeId);
        return data;
    }

    /// <inheritdoc />
    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync()
    {
        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Invalid employee identity from JWT");
            return new();
        }

        return await GetByEmployeeAsync(employeeId);
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> ApproveAsync(ApproveRequest dto)
    {
        _logger.LogInformation("Approving leave request {LeaveRequestId}", dto.Id);

        var c = Client();
        var url = $"api/leave-requests/{dto.Id}/approve";

        var managerId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (managerId == Guid.Empty)
            return ApiResult<bool>.Fail("Invalid manager identity.");

        var payload = new { managerId };

        var resp = await c.PostAsJsonAsync(url, payload);

        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> RejectAsync(RejectRequest dto)
    {
        _logger.LogInformation("Rejecting leave request {LeaveRequestId}", dto.Id);

        var c = Client();
        var url = $"api/leave-requests/{dto.Id}/reject";

        var managerId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (managerId == Guid.Empty)
            return ApiResult<bool>.Fail("Invalid manager identity.");

        dto.Reason ??= "No reason provided";

        var payload = new { ManagerId = managerId, Reason = dto.Reason };

        var resp = await c.PostAsJsonAsync(url, payload);

        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> RetractAsync(RetractRequest dto)
    {
        _logger.LogInformation("Retracting leave request {LeaveRequestId}", dto.Id);

        var c = Client();
        var url = $"api/leave-requests/{dto.Id}/retract";

        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
            return ApiResult<bool>.Fail("Invalid employee identity.");

        var payload = new { employeeId };

        var resp = await c.PostAsJsonAsync(url, payload);

        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    /// <inheritdoc />
    public async Task<List<LeaveRequestListItem>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all leave requests");

        var c = Client();
        var url = "api/leave-requests";
        var data = await c.GetFromJsonAsync<List<LeaveRequestListItem>>(url) ?? new();

        var types = await _leaveTypes.GetAsync();
        foreach (var req in data)
        {
            var lt = types.FirstOrDefault(t => t.Id == req.LeaveTypeId);
            if (lt is not null) req.LeaveTypeName = lt.Name;
        }

        _logger.LogInformation("Fetched {Count} leave requests", data.Count);
        return data;
    }

    /// <inheritdoc />
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        _logger.LogInformation("Fetching all users");

        var c = Client();
        var data = await c.GetFromJsonAsync<List<UserDto>>("api/users") ?? new();

        _logger.LogInformation("Fetched {Count} users", data.Count);
        return data;
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> EditAsync(EditRequest dto)
    {
        _logger.LogInformation("Editing leave request {LeaveRequestId}", dto.Id);

        var c = Client();
        var url = $"api/leave-requests/{dto.Id}/edit";

        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
            return ApiResult<bool>.Fail("Invalid employee identity.");

        dto.EmployeeId = employeeId;

        var resp = await c.PutAsJsonAsync(url, dto);

        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

}

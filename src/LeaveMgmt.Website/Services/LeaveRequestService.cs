using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

public sealed class LeaveRequestService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly AuthService _auth;
    private readonly LeaveTypeService _leaveTypes;
    public LeaveRequestService(IHttpClientFactory httpFactory, AuthService auth, LeaveTypeService leaveTypes)
    {
        _httpFactory = httpFactory;
        _auth = auth;
        _leaveTypes = leaveTypes;
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

    public async Task<ApiResult<Guid>> SubmitAsync(SubmitLeaveRequest dto)
    {
        var c = Client();
        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
            return new();
        dto.EmployeeId = employeeId;
        var resp = await c.PostAsJsonAsync("api/leave-requests", dto);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<Guid>.Fail(await resp.Content.ReadAsStringAsync());

        var result = await resp.Content.ReadFromJsonAsync<SubmitLeaveResponse>();
        
        if (result == null)
            return ApiResult<Guid>.Fail("Empty response");

        return ApiResult<Guid>.Ok(result.Id);
    }

    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync(Guid employeeId)
    {
        var c = Client();

        var url = $"api/leave-requests/by-employee/{employeeId}";
        var data = await c.GetFromJsonAsync<List<LeaveRequestListItem>>(url);

        // Get leave types
        var types = await _leaveTypes.GetAsync();

        // Match LeaveTypeId -> LeaveTypeName
        foreach (var req in data)
        {
            var lt = types.FirstOrDefault(t => t.Id == req.LeaveTypeId);
            if (lt != null)
            {
                req.LeaveTypeName = lt.Name;
            }
        }
        return data ?? new();
    }

    // Overload that figures out the employeeId from the JWT
    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync()
    {
        var employeeId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (employeeId == Guid.Empty)
            return new();

        return await GetByEmployeeAsync(employeeId);
    }

    public async Task<ApiResult<bool>> ApproveAsync(ApproveRequest dto)
    {
        var c = Client();

        // must include Id in the route
        var url = $"api/leave-requests/{dto.Id}/approve";

        // resolve manager id from JWT
        var managerId = Helpers.Helpers.TryGetUserIdFromJwt(LoggedUser.Token);
        if (managerId == Guid.Empty)
            return ApiResult<bool>.Fail("Invalid manager identity.");

        var payload = new { managerId };

        var resp = await c.PostAsJsonAsync(url, payload);

        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }



    public async Task<ApiResult<bool>> RejectAsync(RejectRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests/reject", dto);
        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    public async Task<ApiResult<bool>> RetractAsync(RetractRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests/retract", dto);
        return resp.IsSuccessStatusCode
            ? ApiResult<bool>.Ok(true)
            : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    public async Task<List<LeaveRequestListItem>> GetAllAsync()
    {
        var c = Client();
        var url = "api/leave-requests"; // assumes your API has a GET all endpoint
        var data = await c.GetFromJsonAsync<List<LeaveRequestListItem>>(url) ?? new();

        var types = await _leaveTypes.GetAsync();
        foreach (var req in data)
        {
            var lt = types.FirstOrDefault(t => t.Id == req.LeaveTypeId);
            if (lt != null) req.LeaveTypeName = lt.Name;
        }

        return data;
    }

    // NEW: Get all users for dropdown in ManageRequests
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var c = Client();
        var data = await c.GetFromJsonAsync<List<UserDto>>("api/users");
        return data ?? new();
    }

}

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using LeaveMgmt.Web.Models;

namespace LeaveMgmt.Web.Services;

public sealed class LeaveRequestService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly AuthService _auth;

    public LeaveRequestService(IHttpClientFactory httpFactory, AuthService auth)
    {
        _httpFactory = httpFactory;
        _auth = auth;
    }

    private HttpClient Client()
    {
        var c = _httpFactory.CreateClient("api");
        if (!string.IsNullOrWhiteSpace(_auth.Token))
            c.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _auth.Token);
        return c;
    }

    public async Task<ApiResult<Guid>> SubmitAsync(SubmitLeaveRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests", dto);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<Guid>.Fail(await resp.Content.ReadAsStringAsync());
        var id = await resp.Content.ReadFromJsonAsync<Guid>();
        return ApiResult<Guid>.Ok(id);
    }

    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync(Guid employeeId)
    {
        var c = Client();
        var url = $"api/leave-requests/by-employee/{employeeId}";
        var data = await c.GetFromJsonAsync<List<LeaveRequestListItem>>(url);
        return data ?? new();
    }

    // Keeps the same signature; now derives the Guid from JWT using a proper parser.
    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync()
    {
        var employeeId = TryGetUserIdFromJwt(_auth.Token);
        if (employeeId == Guid.Empty)
            return new(); // or throw, keeping existing behavior lightweight

        return await GetByEmployeeAsync(employeeId);
    }

    private static Guid TryGetUserIdFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return Guid.Empty;

        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);

            // Try common claim types in order: 'sub', NameIdentifier, or custom fallbacks.
            string? id =
                token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ??
                token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                token.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ??
                token.Claims.FirstOrDefault(c => c.Type == "employeeId")?.Value;

            return Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    public async Task<ApiResult<bool>> ApproveAsync(ApproveRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests/approve", dto);
        return resp.IsSuccessStatusCode ? ApiResult<bool>.Ok(true)
                                        : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    public async Task<ApiResult<bool>> RejectAsync(RejectRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests/reject", dto);
        return resp.IsSuccessStatusCode ? ApiResult<bool>.Ok(true)
                                        : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }

    public async Task<ApiResult<bool>> RetractAsync(RetractRequest dto)
    {
        var c = Client();
        var resp = await c.PostAsJsonAsync("api/leave-requests/retract", dto);
        return resp.IsSuccessStatusCode ? ApiResult<bool>.Ok(true)
                                        : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
    }
}

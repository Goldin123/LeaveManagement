using System.Net.Http.Json;
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
    public async Task<List<LeaveRequestListItem>> GetByEmployeeAsync()
    {
        // Derive employeeId from the JWT (no model/property changes)
        var employeeId = TryGetUserIdFromJwt(_auth.Token);
        if (employeeId == Guid.Empty)
            return new(); // or throw if you prefer

        return await GetByEmployeeAsync(employeeId);
    }

    private static Guid TryGetUserIdFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return Guid.Empty;

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return Guid.Empty;

        // Decode payload (middle) and read common claim names
        try
        {
            var payloadJson = System.Text.Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);

            // Try typical claim keys — adjust order if your token differs
            if (doc.RootElement.TryGetProperty("sub", out var sub) &&
                Guid.TryParse(sub.GetString(), out var g1))
                return g1;

            if (doc.RootElement.TryGetProperty("userId", out var uid) &&
                Guid.TryParse(uid.GetString(), out var g2))
                return g2;

            if (doc.RootElement.TryGetProperty("employeeId", out var eid) &&
                Guid.TryParse(eid.GetString(), out var g3))
                return g3;
        }
        catch
        {
            // ignore and return empty
        }

        return Guid.Empty;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
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

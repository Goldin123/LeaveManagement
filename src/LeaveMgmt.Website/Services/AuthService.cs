using LeaveMgmt.Website.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LeaveMgmt.Website.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ProtectedLocalStorage _storage;

        public AuthService(IHttpClientFactory httpFactory, ProtectedLocalStorage storage)
        {
            _httpFactory = httpFactory;
            _storage = storage;
        }

        public UserInfo? CurrentUser { get; private set; }
        public string? Token { get; private set; }

        public async Task<ApiResult<LoginResponse>> LoginAsync(LoginBody body)
        {
            var http = _httpFactory.CreateClient("api");
            var resp = await http.PostAsJsonAsync("api/auth/login", body);
            if (!resp.IsSuccessStatusCode)
                return ApiResult<LoginResponse>.Fail(await resp.Content.ReadAsStringAsync());

            var dto = await resp.Content.ReadFromJsonAsync<LoginResponse>();
            if (dto is null || string.IsNullOrWhiteSpace(dto.Token))
                return ApiResult<LoginResponse>.Fail("Invalid login response.");

            Token = dto.Token;

            // Guard JS during any potential prerender edge cases
            try
            {
                await _storage.SetAsync("jwt", Token);
            }
            catch (InvalidOperationException)
            {
                // Prerendered static render path cannot use JS; ignore and continue.
                // Token is still set in-memory; UI remains functional after circuit connects.
            }

            CurrentUser = new UserInfo { UserName = dto.UserName, Role = dto.Role };
            return ApiResult<LoginResponse>.Ok(dto);
        }

        public async Task<ApiResult<bool>> RegisterAsync(RegisterBody body)
        {
            var http = _httpFactory.CreateClient("api");
            var resp = await http.PostAsJsonAsync("api/auth/register", body);
            return resp.IsSuccessStatusCode
                ? ApiResult<bool>.Ok(true)
                : ApiResult<bool>.Fail(await resp.Content.ReadAsStringAsync());
        }

        public async Task InitializeAsync()
        {
            // ProtectedLocalStorage uses JS interop; it throws during prerender.
            try
            {
                var jwt = await _storage.GetAsync<string>("jwt");
                if (jwt.Success && !string.IsNullOrWhiteSpace(jwt.Value))
                {
                    Token = jwt.Value;
                    CurrentUser ??= new UserInfo { UserName = "Me", Role = "Employee" };
                }
            }
            catch (InvalidOperationException)
            {
                // Running during static prerender – skip. The UI can call this again after
                // interactivity is established, and all other logic remains unchanged.
            }
        }

        public async Task LogoutAsync()
        {
            Token = null;
            CurrentUser = null;

            try
            {
                await _storage.DeleteAsync("jwt");
            }
            catch (InvalidOperationException)
            {
                // Ignore if prerender prevents JS – in-memory state already cleared.
            }
        }
    }
}

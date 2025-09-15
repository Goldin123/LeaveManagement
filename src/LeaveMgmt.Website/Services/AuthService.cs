using LeaveMgmt.Website.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

            // Guard JS during prerender
            try
            {
                await _storage.SetAsync("jwt", Token);
            }
            catch (InvalidOperationException) { }

            var role = TryGetUserRoleFromJwt(Token);
            CurrentUser = new UserInfo { UserName = dto.UserName, Role = role ?? "Employee" };

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
            try
            {
                var jwt = await _storage.GetAsync<string>("jwt");
                if (jwt.Success && !string.IsNullOrWhiteSpace(jwt.Value))
                {
                    Token = jwt.Value;
                    var role = TryGetUserRoleFromJwt(Token);
                    CurrentUser ??= new UserInfo { UserName = "Me", Role = role ?? "Employee" };
                }
            }
            catch (InvalidOperationException) { }
        }

        public async Task LogoutAsync()
        {
            Token = null;
            CurrentUser = null;

            try
            {
                await _storage.DeleteAsync("jwt");
            }
            catch (InvalidOperationException) { }
        }

        private static string? TryGetUserRoleFromJwt(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return null;

            try
            {
                var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);

                // Try common role claim types
                return token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                    ?? token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}

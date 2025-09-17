using LeaveMgmt.Website.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Handles authentication logic in the Blazor website,
/// including login, registration, session restore, and logout.
/// Uses IHttpClientFactory to call the API and ProtectedLocalStorage for persistence.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ProtectedLocalStorage _storage;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IHttpClientFactory httpFactory,
        ProtectedLocalStorage storage,
        ILogger<AuthService> logger)
    {
        _httpFactory = httpFactory;
        _storage = storage;
        _logger = logger;
    }

    public UserInfo? CurrentUser { get; private set; }
    public string? Token { get; private set; }

    public string? GetToken() => Token;

    /// <inheritdoc />
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginBody body)
    {
        _logger.LogInformation("Login attempt for {Email}", body.Email);

        var http = _httpFactory.CreateClient("api");
        var resp = await http.PostAsJsonAsync("api/auth/login", body);

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed for {Email}: {Error}", body.Email, err);
            return ApiResult<LoginResponse>.Fail(err);
        }

        var dto = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        if (dto is null || string.IsNullOrWhiteSpace(dto.Token))
        {
            _logger.LogWarning("Invalid login response for {Email}", body.Email);
            return ApiResult<LoginResponse>.Fail("Invalid login response.");
        }

        Token = dto.Token;
        LoggedUser.Token = Token;

        try
        {
            await _storage.SetAsync("jwt", Token);
            _logger.LogInformation("Stored JWT for {Email}", body.Email);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to persist JWT for {Email}", body.Email);
        }

        var role = TryGetUserRoleFromJwt(Token);
        var name = new JwtSecurityTokenHandler()
            .ReadJwtToken(Token)
            .Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? "Me";

        CurrentUser = new UserInfo { UserName = name, Role = role ?? "Employee" };
        _logger.LogInformation("Login successful for {Email}, role {Role}", body.Email, CurrentUser.Role);

        return ApiResult<LoginResponse>.Ok(dto);
    }

    /// <inheritdoc />
    public async Task<ApiResult<bool>> RegisterAsync(RegisterBody body)
    {
        _logger.LogInformation("Registration attempt for {Email}", body.Email);

        var http = _httpFactory.CreateClient("api");
        var resp = await http.PostAsJsonAsync("api/auth/register", body);

        if (resp.IsSuccessStatusCode)
        {
            _logger.LogInformation("Registration successful for {Email}", body.Email);
            return ApiResult<bool>.Ok(true);
        }
        else
        {
            var err = await resp.Content.ReadAsStringAsync();
            _logger.LogWarning("Registration failed for {Email}: {Error}", body.Email, err);
            return ApiResult<bool>.Fail(err);
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        try
        {
            var jwt = await _storage.GetAsync<string>("jwt");
            if (jwt.Success && !string.IsNullOrWhiteSpace(jwt.Value))
            {
                Token = jwt.Value;
                var role = TryGetUserRoleFromJwt(Token);
                var name = new JwtSecurityTokenHandler()
                    .ReadJwtToken(Token)
                    .Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? "Me";

                CurrentUser ??= new UserInfo { UserName = name, Role = role ?? "Employee" };

                _logger.LogInformation("Restored session for {User}", name);
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to restore session from storage");
        }
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        var user = CurrentUser?.UserName ?? "Unknown";
        Token = null;
        CurrentUser = null;

        try
        {
            await _storage.DeleteAsync("jwt");
            _logger.LogInformation("User {User} logged out", user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to clear session for {User}", user);
        }
    }

    /// <summary>
    /// Extracts the role claim from a JWT token if available.
    /// </summary>
    private static string? TryGetUserRoleFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return null;

        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);

            return token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                ?? token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        }
        catch
        {
            return null;
        }
    }
}

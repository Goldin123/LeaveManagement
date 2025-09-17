using LeaveMgmt.Website.Models;

namespace LeaveMgmt.Website.Services;

/// <summary>
/// Defines authentication operations for the website layer.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Currently authenticated user (if any).
    /// </summary>
    UserInfo? CurrentUser { get; }

    /// <summary>
    /// Returns the JWT token of the authenticated user.
    /// </summary>
    string? GetToken();

    /// <summary>
    /// Attempts to log a user in.
    /// </summary>
    Task<ApiResult<LoginResponse>> LoginAsync(LoginBody body);

    /// <summary>
    /// Attempts to register a new user.
    /// </summary>
    Task<ApiResult<bool>> RegisterAsync(RegisterBody body);

    /// <summary>
    /// Restores a session from persisted storage (if available).
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Logs the current user out and clears session storage.
    /// </summary>
    Task LogoutAsync();
}

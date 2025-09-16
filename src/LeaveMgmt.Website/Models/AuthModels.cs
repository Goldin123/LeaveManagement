using System.ComponentModel.DataAnnotations;

namespace LeaveMgmt.Website.Models;

public class LoginBody
{
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public sealed class RegisterBody
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "Employee";
}
public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
}
public sealed class UserInfo
{
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee";
}

public sealed class ApiResult<T>
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResult<T> Fail(string? error) => new() { Success = false, Error = error };
}

public static class LoggedUser 
{
    public static string? Token { get; set; }
}

public class UserDto 
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

}
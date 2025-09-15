// File: src/LeaveMgmt.Application/DTOs/AuthTokensDto.cs
namespace LeaveMgmt.Application.DTOs;

public sealed class AuthTokensDto
{
    public string AccessToken { get; init; } = default!;
    public DateTime ExpiresUtc { get; init; }
}

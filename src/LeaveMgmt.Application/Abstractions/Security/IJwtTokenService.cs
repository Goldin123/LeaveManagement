namespace LeaveMgmt.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, string email, IEnumerable<string> roles, TimeSpan? lifetime = null);
}

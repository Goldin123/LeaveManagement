namespace LeaveMgmt.Application.Abstractions.Security;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, string email, string fullName, IEnumerable<string> roles, TimeSpan? lifetime = null);
}

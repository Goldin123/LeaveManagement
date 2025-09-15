using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeaveMgmt.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LeaveMgmt.Infrastructure.Security;

internal sealed class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    public string CreateToken(Guid userId, string email, IEnumerable<string> roles, TimeSpan? lifetime = null)
    {
        var issuer = config["Jwt:Issuer"] ?? "leave-mgmt";
        var audience = config["Jwt:Audience"] ?? "leave-mgmt-clients";
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier,   userId.ToString()),
            new(ClaimTypes.Email,            email ?? string.Empty),
        };

        foreach (var r in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, r));
            claims.Add(new Claim("role", r)); // explicit role claim for client-side parsing
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(8)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

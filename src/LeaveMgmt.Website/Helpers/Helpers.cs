using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LeaveMgmt.Website.Helpers
{
    public static class Helpers
    {
        public static Guid TryGetUserIdFromJwt(string? jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
                return Guid.Empty;

            try
            {
                var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);

                string? id =
                    token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ??
                    token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ??
                    token.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ??
                    token.Claims.FirstOrDefault(c => c.Type == "employeeId")?.Value;

                return Guid.TryParse(id, out var g) ? g : Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }

    }


}

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

        /// <summary>
        /// Global list of holidays (can be expanded or loaded from config/db later).
        /// </summary>
        private static readonly List<DateTime> Holidays = new()
        {
            new DateTime(DateTime.Today.Year, 1, 1),   // New Year
            new DateTime(DateTime.Today.Year, 12, 25)  // Christmas
        };

        /// <summary>
        /// Calculates number of working days between From and To (inclusive),
        /// excluding weekends and predefined holidays.
        /// </summary>
        public static int CalculateWorkingDays(DateTime from, DateTime to)
        {
            if (to < from)
                return 0;

            int totalDays = 0;

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                if (Holidays.Any(h => h.Date == day.Date))
                    continue;

                totalDays++;
            }

            return totalDays;
        }
    }
}

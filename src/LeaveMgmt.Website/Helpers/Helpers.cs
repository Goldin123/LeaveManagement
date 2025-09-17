using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LeaveMgmt.Website.Models;
using LeaveMgmt.Website.Services;

namespace LeaveMgmt.Website.Helpers;

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
    /// Calculates number of working days between From and To (inclusive),
    /// excluding weekends and public holidays fetched via HolidayService.
    /// </summary>
    public static async Task<int> CalculateWorkingDaysAsync(DateTime from, DateTime to, IHolidayService holidayService)
    {
        if (to < from)
            return 0;

        int year = from.Year; // assume within one year for now
        var holidays = await holidayService.GetAsync(year);
        var holidayDates = holidays.Select(h => h.Date.Date).ToHashSet();

        int totalDays = 0;

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            if (holidayDates.Contains(day.Date))
                continue;

            totalDays++;
        }

        return totalDays;
    }
}

using System.Text.Json;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain.Holidays;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Repositories;

/// <summary>
/// Repository for retrieving holidays from a JSON file.
/// Reads the configured file path from appsettings.
/// </summary>
public sealed class HolidayRepository(IConfiguration cfg, ILogger<HolidayRepository> logger) : IHolidayRepository
{
    private readonly string _path =  Path.Combine(AppContext.BaseDirectory, "Seeds", "Holidays.json");

    /// <summary>
    /// Get holidays for the specified year.
    /// Loads from a JSON file defined in appsettings.json.
    /// </summary>
    public async Task<IReadOnlyList<Holiday>> GetHolidaysAsync(int year, CancellationToken ct = default)
    {
        try
        {
            // Log attempt
            logger.LogInformation("Loading holidays for year {Year} from {Path}", year, _path);

            // 1) Ensure file exists
            if (!File.Exists(_path))
            {
                logger.LogWarning("Holidays file not found at {Path}", _path);
                return Array.Empty<Holiday>();
            }

            // 2) Deserialize JSON
            using var stream = File.OpenRead(_path);
            var doc = await JsonSerializer.DeserializeAsync<Dictionary<string, List<JsonHoliday>>>(stream, cancellationToken: ct);

            if (doc is null || !doc.TryGetValue(year.ToString(), out var list))
            {
                logger.LogWarning("No holidays found for year {Year} in {Path}", year, _path);
                return Array.Empty<Holiday>();
            }

            // 3) Map to domain model
            var holidays = list.Select(x => new Holiday(x.Name, DateTime.Parse(x.Date))).ToList();

            logger.LogInformation("Loaded {Count} holidays for year {Year}", holidays.Count, year);

            return holidays;
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Failed to load holidays for year {Year} from {Path}", year, _path);
            return Array.Empty<Holiday>();
        }
    }

    /// <summary>
    /// DTO representation of holiday in JSON.
    /// </summary>
    private sealed class JsonHoliday
    {
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }
}

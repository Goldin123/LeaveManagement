using System.Text.Json;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain.Holidays;
using Microsoft.Extensions.Configuration;

namespace LeaveMgmt.Infrastructure.Repositories;

public sealed class HolidayRepository(IConfiguration cfg) : IHolidayRepository
{
    private readonly string _path = cfg["Seed:HolidaysPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "Seeds", "Holidays.json");

    public async Task<IReadOnlyList<Holiday>> GetHolidaysAsync(int year, CancellationToken ct = default)
    {
        if (!File.Exists(_path))
            return Array.Empty<Holiday>();

        using var stream = File.OpenRead(_path);
        var doc = await JsonSerializer.DeserializeAsync<Dictionary<string, List<JsonHoliday>>>(stream, cancellationToken: ct);

        if (doc is null || !doc.TryGetValue(year.ToString(), out var list))
            return Array.Empty<Holiday>();

        return list.Select(x => new Holiday(x.Name, DateTime.Parse(x.Date))).ToList();
    }

    private sealed class JsonHoliday
    {
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
    }
}

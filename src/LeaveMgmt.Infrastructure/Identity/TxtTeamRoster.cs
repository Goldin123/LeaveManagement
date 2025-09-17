using LeaveMgmt.Application.Abstractions.Identity;
using Microsoft.Extensions.Configuration;

namespace LeaveMgmt.Infrastructure.Identity;

public sealed class TxtTeamRoster(IConfiguration cfg) : ITeamRoster
{
    private readonly Lazy<Dictionary<string, HashSet<string>>> _map = new(() => new(StringComparer.OrdinalIgnoreCase));
    private bool _loaded;

    public bool TryGetRolesFor(string email, out IReadOnlyCollection<string> roles)
    {
        EnsureLoaded();
        if (_map.Value.TryGetValue(email, out var set))
        {
            roles = set;
            return true;
        }
        roles = Array.Empty<string>();
        return false;
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        // Prefer configured file paths if they exist, otherwise fall back to Seeds folder
        TryLoad(cfg["Teams:Dev"], "Employee", "Dev.txt");
        TryLoad(cfg["Teams:Management"], "Manager", "Managment.txt"); // support proper spelling
        TryLoad(cfg["Teams:Managment"], "Manager", "Managment.txt");  // support current spelling
        TryLoad(cfg["Teams:Support"], "Support", "Support.txt");

        _loaded = true;
    }

    private void TryLoad(string? configuredPath, string role, string defaultFile)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            LoadFile(configuredPath, role);
            return;
        }

        var fallback = Path.Combine(AppContext.BaseDirectory, "Seeds", defaultFile);
        if (File.Exists(fallback))
        {
            LoadFile(fallback, role);
        }
    }

    private void LoadFile(string path, string role)
    {
        if (!File.Exists(path)) return;

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            // Skip headings
            if (line.StartsWith("Team Lead", StringComparison.OrdinalIgnoreCase)) continue;
            if (line.StartsWith("Team Members", StringComparison.OrdinalIgnoreCase)) continue;
            if (line.StartsWith("Full Name", StringComparison.OrdinalIgnoreCase)) continue;

            // CSV: Full Name,Employee Number,Email Address,Cellphone Number
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 3) continue;

            var email = parts[2].Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) continue;

            if (!_map.Value.TryGetValue(email, out var set))
                _map.Value[email] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            set.Add(role);
        }
    }
}

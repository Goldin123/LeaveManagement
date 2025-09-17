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

        // Default to files under Infrastructure\Seeds\*.txt if not provided
        var root = AppContext.BaseDirectory; // bin\[Debug|Release]\net9.0\
        string DefaultPath(string file) => Path.Combine(root, "Seeds", file);

        LoadFile(DefaultPath("Dev.txt"), "Employee");
        LoadFile(DefaultPath("Managment.txt"), "Manager");
        LoadFile(DefaultPath("Support.txt"), "Support");

        _loaded = true;
    }

    private void LoadFile(string path, string role)
    {
        if (!File.Exists(path)) return;

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;

            // Skip headings in your sample files
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

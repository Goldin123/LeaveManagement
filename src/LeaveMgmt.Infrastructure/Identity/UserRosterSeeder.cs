using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Identity;

public sealed class UserRosterSeeder(
    ILogger<UserRosterSeeder> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration cfg) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Opt-in via config: "Seed:Users": true
        var enabled = bool.TryParse(cfg["Seed:Users"], out var v) && v;
        if (!enabled) return;

        using var scope = scopeFactory.CreateScope();
        var roster = scope.ServiceProvider.GetRequiredService<ITeamRoster>();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Collect email -> fullName from roster files
        var people = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Accumulate(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            foreach (var line in File.ReadAllLines(path))
            {
                var raw = line.Trim();
                if (raw.Length == 0 || raw.StartsWith("#")) continue;

                // Skip headers and labels
                if (raw.StartsWith("Team Lead", StringComparison.OrdinalIgnoreCase)) continue;
                if (raw.StartsWith("Team Members", StringComparison.OrdinalIgnoreCase)) continue;
                if (raw.StartsWith("Full Name", StringComparison.OrdinalIgnoreCase)) continue;

                // CSV: Full Name,Employee Number,Email,Cellphone
                var parts = raw.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 3) continue;

                var fullName = parts[0];
                var email = parts[2].ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(email)) continue;

                people[email] = fullName;
            }
        }

        Accumulate(cfg["Teams:Dev"]);
        Accumulate(cfg["Teams:Management"] ?? cfg["Teams:Managment"]);
        Accumulate(cfg["Teams:Support"]);

        var (hash, salt) = hasher.Hash(cfg["Seed:DefaultPassword"] ?? "ChangeMe123!");

        foreach (var (email, fullName) in people)
        {
            if (!roster.TryGetRolesFor(email, out var roles) || roles.Count == 0)
                continue;

            var existing = await users.GetByEmailAsync(email, cancellationToken);
            if (!existing.IsSuccess)
            {
                logger.LogWarning("Skip seeding {Email}: {Err}", email, existing.Error);
                continue;
            }
            if (existing.Value is not null) continue;

            var u = new User(email, fullName, hash, salt, roles);
            var created = await users.CreateAsync(u, cancellationToken);
            if (!created.IsSuccess)
                logger.LogWarning("Failed to seed {Email}: {Err}", email, created.Error);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

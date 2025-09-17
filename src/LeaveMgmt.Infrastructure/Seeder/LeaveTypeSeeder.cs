using System.Text.Json;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain.LeaveTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Seeder;

public sealed class LeaveTypeSeeder(
    ILogger<LeaveTypeSeeder> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration cfg) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var enabled = bool.TryParse(cfg["Seed:LeaveTypes"], out var v) && v;
        if (!enabled) return;

        var path = cfg["Seed:LeaveTypesPath"]
                   ?? Path.Combine(AppContext.BaseDirectory, "Seeds", "LeaveTypes.json");

        if (!File.Exists(path))
        {
            logger.LogWarning("LeaveTypes.json not found at {Path}", path);
            return;
        }

        using var stream = File.OpenRead(path);
        var leaveTypes = await JsonSerializer.DeserializeAsync<List<LeaveTypeSeed>>(stream, cancellationToken: cancellationToken);
        if (leaveTypes is null) return;

        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ILeaveTypeRepository>();

        var existing = await repo.GetAllAsync(cancellationToken);
        if (!existing.IsSuccess)
        {
            logger.LogWarning("Could not fetch LeaveTypes: {Err}", existing.Error);
            return;
        }

        foreach (var item in leaveTypes)
        {
            if (existing.Value!.Any(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
                continue;

            var lt = new LeaveType(item.Name, item.MaxDaysPerRequest);
            var created = await repo.CreateAsync(lt, cancellationToken);
            if (!created.IsSuccess)
                logger.LogWarning("Failed seeding {Name}: {Err}", item.Name, created.Error);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed class LeaveTypeSeed
    {
        public string Name { get; set; } = string.Empty;
        public int MaxDaysPerRequest { get; set; }
    }
}

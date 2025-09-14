using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Infrastructure.Messaging;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LeaveMgmt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // ---- EF Core ----
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? "Data Source=:memory:"; // test fallback
        services.AddDbContext<LeaveMgmtDbContext>(opt => opt.UseSqlite(cs)); // or UseSqlServer(cs)

        // ---- Repositories ----
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();

        // ---- Redis Bus + Outbox Dispatcher ----
        var redis = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis));
        services.AddSingleton<IEventBus, RedisEventBus>();
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}

// NEW:
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Infrastructure.Identity;
using LeaveMgmt.Infrastructure.Messaging;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Repositories;
using LeaveMgmt.Infrastructure.Security;
using LeaveMgmt.Infrastructure.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace LeaveMgmt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // ---- EF Core : SQL Server (code-first migrations live in this assembly) ----
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing connection string 'DefaultConnection'.");

        services.AddDbContext<LeaveMgmtDbContext>(opt =>
            opt.UseSqlServer(cs, sql =>
            {
                sql.MigrationsAssembly(typeof(LeaveMgmtDbContext).Assembly.FullName);
            }));

        // ---- Repositories ----
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>(); // ✅ you already had this
        services.AddScoped<IHolidayRepository, HolidayRepository>();

        // ---- Security (hashing + JWT) ----
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>(); // uses your interface

        // ---- Roster (Dev/Management/Support .txt files) ----
        services.AddSingleton<ITeamRoster, TxtTeamRoster>();        // ✅ add this

        // ---- Redis Bus + Outbox Dispatcher ----
        var redis = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redis));
        services.AddSingleton<IEventBus, RedisEventBus>();
        services.AddHostedService<OutboxDispatcher>();              // singleton, creates scopes internally

        // ---- (Optional) Seed roster users on startup (enable with "Seed:Users": true) ----
        services.AddHostedService<UserRosterSeeder>();               // ✅ add this if you want auto-seed
        services.AddHostedService<LeaveTypeSeeder>();

        return services;
    }
}

using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Domain.Users;
using LeaveMgmt.Infrastructure.Identity;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Repositories;
using LeaveMgmt.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class UserRosterSeederTests
{
    [Fact] // Functional
    public async Task Seeder_Should_Insert_Users_From_Rosters()
    {
        // temp roster files
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "seed_users_" + Guid.NewGuid()));
        var dev = Path.Combine(dir.FullName, "Dev.txt");
        var mgmt = Path.Combine(dir.FullName, "Managment.txt");
        var sup = Path.Combine(dir.FullName, "Support.txt");

        File.WriteAllText(dev,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Ella Jefferson,2005,ellajefferson@acme.com,+27 55 979 367");

        File.WriteAllText(mgmt,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Colin Horton,3,colinhorton@acme.com,+27 ...");

        File.WriteAllText(sup,
@"Team Members
Full Name,Employee Number,Email Address,Cellphone Number
Amy Burns,2012,amyburns@acme.com,+27 ...");

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:Dev"] = dev,
                ["Teams:Management"] = mgmt,
                ["Teams:Support"] = sup,
                ["Seed:Users"] = "true",
                ["Seed:DefaultPassword"] = "ChangeMe123!"
            }).Build();

        // single in-memory sqlite connection
        var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(cfg);

        services.AddDbContext<LeaveMgmtDbContext>(o => o.UseSqlite(conn));

        // 🔹 Register logging so ILogger<T> can be injected into repositories
        services.AddLogging();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<ITeamRoster, TxtTeamRoster>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        var provider = services.BuildServiceProvider();

        // Ensure DB created
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LeaveMgmtDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        // run the seeder
        var seeder = new UserRosterSeeder(
            new NullLogger<UserRosterSeeder>(),
            provider.GetRequiredService<IServiceScopeFactory>(),
            cfg);

        await seeder.StartAsync(CancellationToken.None);

        // verify users exist
        using var verifyScope = provider.CreateScope();
        var ctx = verifyScope.ServiceProvider.GetRequiredService<LeaveMgmtDbContext>();
        var users = await ctx.Users.ToListAsync();

        users.Should().Contain(u => u.Email == "ellajefferson@acme.com");
        users.Should().Contain(u => u.Email == "colinhorton@acme.com");
        users.Should().Contain(u => u.Email == "amyburns@acme.com");
    }
}

using System.Data.Common;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure;

public sealed class SqliteInMemoryFixture : IAsyncLifetime
{
    private DbConnection? _conn;

    public LeaveMgmtDbContext DbContext { get; private set; } = default!;
    public LeaveRequestRepository LeaveRequestRepository { get; private set; } = default!;
    public LeaveTypeRepository LeaveTypeRepository { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        await _conn.OpenAsync();

        var options = new DbContextOptionsBuilder<LeaveMgmtDbContext>()
            .UseSqlite(_conn)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;

        DbContext = new LeaveMgmtDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();

        LeaveRequestRepository = new LeaveRequestRepository(DbContext);
        LeaveTypeRepository = new LeaveTypeRepository(DbContext);
    }

    public async ValueTask DisposeAsync()
    {
        if (DbContext is not null) await DbContext.DisposeAsync();
        if (_conn is not null) await _conn.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        DbContext.ChangeTracker.Clear();
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }
}

[CollectionDefinition(nameof(SqliteCollection))]
public sealed class SqliteCollection : ICollectionFixture<SqliteInMemoryFixture> { }

public abstract class IntegrationTestBase
{
    protected IntegrationTestBase(SqliteInMemoryFixture fixture) => Fixture = fixture;
    protected SqliteInMemoryFixture Fixture { get; }
    protected Task ResetDbAsync() => Fixture.ResetAsync();
}

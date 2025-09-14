// File: LeaveMgmt.Infrastructure.IntegrationTests/Outbox/OutboxCaptureTests.cs
using System;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.ValueObjects;
using LeaveMgmt.Infrastructure.Persistence;
using LeaveMgmt.Infrastructure.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class OutboxCaptureTests
{
    [Fact] // Integration
    public async Task SaveChanges_Should_Write_Outbox_For_LeaveRequestApproved()
    {
        // Arrange: real in-memory Sqlite database + our DbContext
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();

        var options = new DbContextOptionsBuilder<LeaveMgmtDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var db = new LeaveMgmtDbContext(options);
        await db.Database.EnsureCreatedAsync(); // builds tables picked up via ApplyConfigurationsFromAssembly
        // (LeaveMgmtDbContext inherits AppDbContext which applies configurations) :contentReference[oaicite:0]{index=0} :contentReference[oaicite:1]{index=1}

        // Seed a LeaveType and LeaveRequest
        var lt = LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure.DomainBuilders.BuildLeaveType();
        await db.LeaveTypes.AddAsync(lt);
        await db.SaveChangesAsync();

        var req = LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure.DomainBuilders
                    .BuildLeaveRequest(leaveTypeAggregate: lt);
        req.Submit();
        await db.LeaveRequests.AddAsync(req);
        await db.SaveChangesAsync();

        // Act: cause a domain event and save (DbContext SaveChanges adds to Outbox)
        var manager = new ManagerId(Guid.NewGuid());
        req.Approve(manager);
        await db.SaveChangesAsync();

        // Assert: Outbox has an approval message
        var count = await db.Set<OutboxMessage>()
                            .CountAsync(o => o.Topic == "LeaveRequestApproved");
        count.Should().BeGreaterThan(0);
    }
}

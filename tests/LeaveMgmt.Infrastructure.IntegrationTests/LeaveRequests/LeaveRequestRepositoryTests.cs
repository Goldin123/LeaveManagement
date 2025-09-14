using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure;
using Xunit;

namespace LeaveMgmt.Infrastructure.IntegrationTests.LeaveRequests;

[Collection(nameof(SqliteCollection))]
public class LeaveRequestRepositoryTests : IntegrationTestBase
{
    public LeaveRequestRepositoryTests(SqliteInMemoryFixture fixture) : base(fixture) { }

    private async Task<LeaveType> SeedLeaveTypeAsync(string name = "Annual")
    {
        var lt = DomainBuilders.BuildLeaveType(name, "desc", 15);
        var create = await Fixture.LeaveTypeRepository.CreateAsync(lt, TestContext.Current.CancellationToken);
        create.IsSuccess.Should().BeTrue(create.Error);
        return create.Value!;
    }

    [Fact]
    public async Task Add_Then_GetById_Should_Roundtrip()
    {
        await ResetDbAsync();

        var lt = await SeedLeaveTypeAsync();
        var req = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt);

        await Fixture.DbContext.LeaveRequests.AddAsync(req, TestContext.Current.CancellationToken);
        await Fixture.DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await Fixture.LeaveRequestRepository.GetByIdAsync(req.Id, TestContext.Current.CancellationToken);
        found.IsSuccess.Should().BeTrue(found.Error);
        found.Value!.Id.Should().Be(req.Id);
        found.Value!.LeaveTypeId.Should().Be(lt.Id);
    }

    [Fact]
    public async Task GetByEmployee_Should_Return_Only_Target_Employee()
    {
        await ResetDbAsync();

        var lt = await SeedLeaveTypeAsync();
        var emp1 = new EmployeeId(Guid.NewGuid());
        var emp2 = new EmployeeId(Guid.NewGuid());

        var r1 = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt, employeeId: emp1);
        var r2 = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt, employeeId: emp1);
        var r3 = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt, employeeId: emp2);

        await Fixture.DbContext.LeaveRequests.AddRangeAsync(new[] { r1, r2, r3 }, TestContext.Current.CancellationToken);
        await Fixture.DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var res = await Fixture.LeaveRequestRepository.GetByEmployeeAsync(emp1, TestContext.Current.CancellationToken);
        res.IsSuccess.Should().BeTrue(res.Error);
        res.Value!.Should().HaveCount(2);
        res.Value!.All(x => x.EmployeeId.Equals(emp1)).Should().BeTrue();
    }
    [Fact]
    public async Task Approve_Should_Update_Status_And_Audit()
    {
        await ResetDbAsync();

        // Persist a real LeaveType and use the aggregate when building the request
        var lt = await SeedLeaveTypeAsync();

        var req = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt);
        req.Submit().IsSuccess.Should().BeTrue();

        await Fixture.DbContext.LeaveRequests.AddAsync(req);
        await Fixture.DbContext.SaveChangesAsync();

        var manager = new ManagerId(Guid.NewGuid());
        req.Approve(manager).IsSuccess.Should().BeTrue();

        var updateRes = await Fixture.LeaveRequestRepository.UpdateAsync(req, TestContext.Current.CancellationToken);
        updateRes.IsSuccess.Should().BeTrue(updateRes.Error);

        // Reload and assert
        var found = await Fixture.LeaveRequestRepository.GetByIdAsync(req.Id, TestContext.Current.CancellationToken);
        found.IsSuccess.Should().BeTrue(found.Error);

        var r = found.Value!;
        r.Status.Should().Be(LeaveStatus.Approved);
        r.DecidedUtc.Should().NotBeNull();
        r.ApprovedBy.HasValue.Should().BeTrue();

        // Robust compare without format overloads
        r.ApprovedBy!.Value.ToString().Should().Be(manager.Value.ToString());
    }

    [Fact]
    public async Task Reject_Should_Update_Status_And_Audit()
    {
        await ResetDbAsync();

        // Persist LeaveType and use the aggregate to keep FK valid
        var lt = await SeedLeaveTypeAsync();

        var req = DomainBuilders.BuildLeaveRequest(leaveTypeAggregate: lt);
        req.Submit().IsSuccess.Should().BeTrue();

        await Fixture.DbContext.LeaveRequests.AddAsync(req);
        await Fixture.DbContext.SaveChangesAsync();

        var manager = new ManagerId(Guid.NewGuid());
        req.Reject(manager, "Not enough balance").IsSuccess.Should().BeTrue();

        var updateRes = await Fixture.LeaveRequestRepository.UpdateAsync(req, TestContext.Current.CancellationToken);
        updateRes.IsSuccess.Should().BeTrue(updateRes.Error);

        var found = await Fixture.LeaveRequestRepository.GetByIdAsync(req.Id, TestContext.Current.CancellationToken);
        found.IsSuccess.Should().BeTrue(found.Error);

        var r = found.Value!;
        r.Status.Should().Be(LeaveStatus.Rejected);
        r.DecidedUtc.Should().NotBeNull();
        r.ApprovedBy.HasValue.Should().BeTrue();

        // Robust compare without format overloads
        r.ApprovedBy!.Value.ToString().Should().Be(manager.Value.ToString());
    }

}

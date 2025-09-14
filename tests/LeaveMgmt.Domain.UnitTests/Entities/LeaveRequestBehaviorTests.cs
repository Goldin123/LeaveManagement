using FluentAssertions;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using Xunit;

namespace LeaveMgmt.Domain.UnitTests.Entities;

public class LeaveRequestBehaviorTests
{
    private static LeaveType Annual() => new("Annual", 15);

    [Fact]
    public void Submit_FromDraft_Succeeds()
    {
        var emp = new EmployeeId(Guid.NewGuid());
        var req = new LeaveRequest(emp, Annual(), new(new(2025, 1, 1), new(2025, 1, 3)), "Trip");

        var res = req.Submit();

        res.IsSuccess.Should().BeTrue();
        req.Status.Should().Be(LeaveStatus.Submitted);
        req.SubmittedUtc.Should().NotBeNull();
    }

    //[Fact]
    //public void Approve_FromSubmitted_Succeeds()
    //{
    //    var emp = new EmployeeId(Guid.NewGuid());
    //    var manager = new ManagerId(Guid.NewGuid());
    //    var req = new LeaveRequest(emp, Annual(), new(new(2025, 1, 1), new(2025, 1, 3)), "Trip");
    //    req.Submit();

    //    var res = req.Approve(manager);

    //    res.IsSuccess.Should().BeTrue();
    //    req.Status.Should().Be(LeaveStatus.Approved);
    //    req.ApprovedBy!.Value.Should().Be(manager.Value);
    //}

    [Fact]
    public void Reject_FromSubmitted_Succeeds()
    {
        var emp = new EmployeeId(Guid.NewGuid());
        var manager = new ManagerId(Guid.NewGuid());
        var req = new LeaveRequest(emp, Annual(), new(new(2025, 1, 1), new(2025, 1, 3)), "Trip");
        req.Submit();

        var res = req.Reject(manager, "No coverage");

        res.IsSuccess.Should().BeTrue();
        req.Status.Should().Be(LeaveStatus.Rejected);
    }

    [Fact]
    public void Retract_ByOwner_WhenSubmitted_Succeeds()
    {
        var emp = new EmployeeId(Guid.NewGuid());
        var req = new LeaveRequest(emp, Annual(), new(new(2025, 1, 1), new(2025, 1, 3)), "Trip");
        req.Submit();

        var res = req.Retract(emp);

        res.IsSuccess.Should().BeTrue();
        req.Status.Should().Be(LeaveStatus.Retracted);
    }

    [Fact]
    public void Submit_FromNonDraft_Fails()
    {
        var emp = new EmployeeId(Guid.NewGuid());
        var req = new LeaveRequest(emp, Annual(), new(new(2025, 1, 1), new(2025, 1, 3)), "Trip");
        req.Submit();

        var res = req.Submit();

        res.IsSuccess.Should().BeFalse();
    }
}

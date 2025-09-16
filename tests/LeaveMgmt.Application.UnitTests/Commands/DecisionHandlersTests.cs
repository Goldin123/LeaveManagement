using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using LeaveRequestRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveRequestRepository;

public sealed class DecisionHandlersTests
{
    private static LeaveRequest NewSubmittedRequest()
    {
        var lt = new LeaveType("Annual",  30);
        var req = new LeaveRequest(
            new EmployeeId(Guid.NewGuid()),
            lt,
            new DateRange(DateOnly.FromDateTime(DateTime.UtcNow.Date),
                          DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2))),
            "Trip");
        req.Submit();
        return req;
    }

    [Fact] // Unit
    public async Task Approve_Should_Set_Status_And_Manager()
    {
        var repo = new Mock<LeaveRequestRepo>();
        var agg = NewSubmittedRequest();

        repo.Setup(r => r.GetByIdAsync(agg.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LeaveRequest?>.Success(agg));
        repo.Setup(r => r.UpdateAsync(agg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ApproveLeaveHandler(repo.Object);

        var managerId = Guid.NewGuid();
        var res = await handler.Handle(new ApproveLeaveCommand(agg.Id, managerId), default);

        res.IsSuccess.Should().BeTrue(res.Error);
        agg.Status.Should().Be(LeaveStatus.Approved);
        agg.ApprovedBy!.Value.ToString().Should().Be(managerId.ToString());
    }

    [Fact] // Unit
    public async Task Reject_Should_Set_Status_And_Manager()
    {
        var repo = new Mock<LeaveRequestRepo>();
        var agg = NewSubmittedRequest();

        repo.Setup(r => r.GetByIdAsync(agg.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LeaveRequest?>.Success(agg));
        repo.Setup(r => r.UpdateAsync(agg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new RejectLeaveHandler(repo.Object);

        var managerId = Guid.NewGuid();
        var res = await handler.Handle(new RejectLeaveCommand(agg.Id, managerId, "No balance"), default);

        res.IsSuccess.Should().BeTrue(res.Error);
        agg.Status.Should().Be(LeaveStatus.Rejected);
        agg.ApprovedBy!.Value.ToString().Should().Be(managerId.ToString());
    }

    [Fact] // Unit
    public async Task Retract_Should_Set_Status_Retracted_When_Owner()
    {
        var repo = new Mock<LeaveRequestRepo>();
        var agg = NewSubmittedRequest();

        repo.Setup(r => r.GetByIdAsync(agg.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<LeaveRequest?>.Success(agg));
        repo.Setup(r => r.UpdateAsync(agg, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new RetractLeaveHandler(repo.Object);

        var res = await handler.Handle(new RetractLeaveCommand(agg.Id, agg.EmployeeId.Value), default);

        res.IsSuccess.Should().BeTrue(res.Error);
        agg.Status.Should().Be(LeaveStatus.Retracted);
    }
}

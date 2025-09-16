using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Domain;

public sealed class AuthorizationBehaviorTests
{
    private static Mock<ICurrentUser> User(Guid id, bool auth = true, params string[] roles)
    {
        var m = new Mock<ICurrentUser>();
        m.SetupGet(x => x.IsAuthenticated).Returns(auth);
        m.SetupGet(x => x.UserId).Returns(id);
        m.Setup(x => x.IsInRole(It.IsAny<string>()))
         .Returns((string r) => roles.Contains(r, StringComparer.OrdinalIgnoreCase));
        return m;
    }

    private static Task<Result> OkNext() => Task.FromResult(Result.Success());
    private static Task<Result<Guid>> OkNextGuid() => Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));

    [Fact] // Unit
    public async Task Submit_Should_Fail_When_Not_Owner()
    {
        var current = User(Guid.NewGuid()); // caller
        var behavior = new AuthorizationBehavior<SubmitLeaveRequestCommand, Result<Guid>>(current.Object);

        var cmd = new SubmitLeaveRequestCommand(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), "r");

        var res = await behavior.Handle(cmd, default, OkNextGuid);

        res.IsSuccess.Should().BeFalse();
    }

    [Fact] // Unit
    public async Task Approve_Should_Fail_When_Not_Manager()
    {
        var current = User(Guid.NewGuid(), roles: "Employee");
        var behavior = new AuthorizationBehavior<ApproveLeaveCommand, Result>(current.Object);

        var res = await behavior.Handle(new ApproveLeaveCommand(Guid.NewGuid(), Guid.NewGuid()), default, OkNext);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Contain("Manager");
    }

    [Fact] // Unit
    public async Task Approve_Should_Pass_For_Manager()
    {
        var current = User(Guid.NewGuid(), roles: "Manager");
        var behavior = new AuthorizationBehavior<ApproveLeaveCommand, Result>(current.Object);

        var res = await behavior.Handle(new ApproveLeaveCommand(Guid.NewGuid(), Guid.NewGuid()), default, OkNext);

        res.IsSuccess.Should().BeTrue(res.Error);
    }

    [Fact] // Unit
    public async Task Retract_Should_Fail_When_Not_Owner()
    {
        var current = User(Guid.NewGuid());
        var behavior = new AuthorizationBehavior<RetractLeaveCommand, Result>(current.Object);

        var cmd = new RetractLeaveCommand(Guid.NewGuid(), Guid.NewGuid()); // different owner
        var res = await behavior.Handle(cmd, default, OkNext);

        res.IsSuccess.Should().BeFalse();
    }

    [Fact] // Unit
    public async Task Reject_Should_Pass_For_Admin()
    {
        var current = User(Guid.NewGuid(), roles: "Admin");
        var behavior = new AuthorizationBehavior<RejectLeaveCommand, Result>(current.Object);

        var res = await behavior.Handle(new RejectLeaveCommand(Guid.NewGuid(), Guid.NewGuid(), "n/a"), default, OkNext);

        res.IsSuccess.Should().BeTrue(res.Error);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Application.Validation;
using LeaveMgmt.Domain;

public sealed class ValidationBehaviorTests
{
    [Fact] // Unit
    public async Task Should_Return_Failure_When_Command_Invalid()
    {
        var validator = new SubmitLeaveRequestValidator(); // Start > End will fail
        var behavior = new ValidationBehavior<SubmitLeaveRequestCommand, Result<Guid>>(
            new[] { validator });

        var cmd = new SubmitLeaveRequestCommand(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "r");

        var res = await behavior.Handle(cmd, CancellationToken.None,
            () => Task.FromResult(Result<Guid>.Success(Guid.NewGuid())));

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact] // Unit
    public async Task Should_Pass_Through_When_Valid()
    {
        var validator = new SubmitLeaveRequestValidator();
        var behavior = new ValidationBehavior<SubmitLeaveRequestCommand, Result<Guid>>(
            new[] { validator });

        var cmd = new SubmitLeaveRequestCommand(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            "ok");

        var res = await behavior.Handle(cmd, CancellationToken.None,
            () => Task.FromResult(Result<Guid>.Success(Guid.NewGuid())));

        res.IsSuccess.Should().BeTrue(res.Error);
    }
}

// File: LeaveMgmt.Application.UnitTests/Commands/SubmitLeaveRequestHandlerTests.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Application.Abstractions; // IRequest/IRequestHandler if needed
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
// IMPORTANT: match the namespace where your interfaces live:
using LeaveTypeRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveTypeRepository;
using LeaveRequestRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveRequestRepository;

public sealed class SubmitLeaveRequestHandlerTests
{
    [Fact] // Unit
    public async Task Handle_Should_Create_Submit_And_Return_Id_When_Valid()
    {
        // Arrange
        var lt = new LeaveType("Annual", 30); // assumes public ctor as used across your builders
        var leaveTypes = new Mock<LeaveTypeRepo>();
        leaveTypes.Setup(x => x.GetByIdAsync(lt.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result<LeaveType>.Success(lt));

        var leaveRequests = new Mock<LeaveRequestRepo>();

        LeaveRequest? saved = null;
        leaveRequests.Setup(x => x.CreateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
                     .Callback<LeaveRequest, CancellationToken>((r, _) => saved = r)
                     .ReturnsAsync((LeaveRequest r, CancellationToken _) => Result<LeaveRequest>.Success(r));

        var handler = new SubmitLeaveRequestHandler(leaveRequests.Object, leaveTypes.Object);

        var cmd = new SubmitLeaveRequestCommand(
            EmployeeId: Guid.NewGuid(),
            LeaveTypeId: lt.Id,
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            Reason: "Family event");

        // Act
        var res = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeTrue(res.Error);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(LeaveStatus.Submitted);
        res.Value.Should().Be(saved.Id);
    }

    [Fact] // Unit
    public async Task Handle_Should_Return_Failure_When_LeaveType_NotFound()
    {
        var leaveTypes = new Mock<LeaveTypeRepo>();
        leaveTypes.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result<LeaveType>.Failure("not found"));

        var leaveRequests = new Mock<LeaveRequestRepo>();
        var handler = new SubmitLeaveRequestHandler(leaveRequests.Object, leaveTypes.Object);

        var cmd = new SubmitLeaveRequestCommand(Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            "Reason");

        var res = await handler.Handle(cmd, default);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().NotBeNullOrWhiteSpace();
    }
}

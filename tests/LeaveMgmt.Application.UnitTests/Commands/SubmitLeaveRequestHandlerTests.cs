using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using LeaveTypeRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveTypeRepository;
using LeaveRequestRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveRequestRepository;

public sealed class SubmitLeaveRequestHandlerTests
{
    [Fact] // Unit
    public async Task Handle_Should_Create_Submit_And_Return_Id_When_Valid()
    {
        // Arrange
        var lt = new LeaveType("Annual", 30);
        var leaveTypes = new Mock<LeaveTypeRepo>();
        leaveTypes.Setup(x => x.GetByIdAsync(lt.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result<LeaveType>.Success(lt));

        var leaveRequests = new Mock<LeaveRequestRepo>();
        LeaveRequest? saved = null;

        leaveRequests.Setup(x => x.GetByEmployeeAsync(It.IsAny<EmployeeId>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<IReadOnlyList<LeaveRequest>>.Success(new List<LeaveRequest>()));

        leaveRequests.Setup(x => x.CreateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
                     .Callback<LeaveRequest, CancellationToken>((r, _) => saved = r)
                     .ReturnsAsync((LeaveRequest r, CancellationToken _) => Result<LeaveRequest>.Success(r));

        var logger = new Mock<ILogger<SubmitLeaveRequestHandler>>();

        var handler = new SubmitLeaveRequestHandler(
            leaveRequests.Object,
            leaveTypes.Object,
            logger.Object);

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
        // Arrange
        var leaveTypes = new Mock<LeaveTypeRepo>();
        leaveTypes.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result<LeaveType>.Failure("not found"));

        var leaveRequests = new Mock<LeaveRequestRepo>();
        var logger = new Mock<ILogger<SubmitLeaveRequestHandler>>();

        var handler = new SubmitLeaveRequestHandler(
            leaveRequests.Object,
            leaveTypes.Object,
            logger.Object);

        var cmd = new SubmitLeaveRequestCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            "Reason");

        // Act
        var res = await handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact] // Unit
    public async Task Handle_Should_Fail_When_Policy_Blocks_Overlapping_Request()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var lt = new LeaveType("Annual", 30);

        // Existing approved request
        var existing = new LeaveRequest(new EmployeeId(employeeId), lt,
            new DateRange(DateOnly.FromDateTime(DateTime.Today),
                          DateOnly.FromDateTime(DateTime.Today.AddDays(2))),
            "Existing leave");
        existing.Submit();
        existing.Approve(new ManagerId(Guid.NewGuid()));

        var leaveTypes = new Mock<LeaveTypeRepo>();
        leaveTypes.Setup(x => x.GetByIdAsync(lt.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Result<LeaveType>.Success(lt));

        var leaveRequests = new Mock<LeaveRequestRepo>();
        leaveRequests.Setup(x => x.GetByEmployeeAsync(new EmployeeId(employeeId), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result<IReadOnlyList<LeaveRequest>>.Success(new List<LeaveRequest> { existing }));

        var logger = new Mock<ILogger<SubmitLeaveRequestHandler>>();

        var handler = new SubmitLeaveRequestHandler(
            leaveRequests.Object,
            leaveTypes.Object,
            logger.Object);

        // New overlapping request
        var cmd = new SubmitLeaveRequestCommand(
            employeeId,
            lt.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)), // overlaps
            DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            "Vacation");

        // Act
        var res = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Be("Leave request violates company leave policy.");
    }
}

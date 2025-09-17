using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Application.Queries.LeaveRequests.GetByEmployee;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using LeaveRequestRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveRequestRepository;

public sealed class GetLeaveRequestsByEmployeeTests
{
    [Fact] // Unit
    public async Task Should_Return_Only_Target_Employee()
    {
        var repo = new Mock<LeaveRequestRepo>();
        var logger = new Mock<ILogger<GetLeaveRequestsByEmployeeHandler>>();

        var lt = new LeaveType("Annual", 30);
        var e1 = new EmployeeId(Guid.NewGuid());
        var e2 = new EmployeeId(Guid.NewGuid());

        LeaveRequest NewReq(EmployeeId e, int days) =>
            new LeaveRequest(e, lt,
                new DateRange(DateOnly.FromDateTime(DateTime.UtcNow.Date),
                              DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(days))),
                "r");

        var r1 = NewReq(e1, 1); r1.Submit();
        var r2 = NewReq(e1, 2); r2.Submit();
        var r3 = NewReq(e2, 3); r3.Submit();

        repo.Setup(r => r.GetByEmployeeAsync(e1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<LeaveRequest>>.Success(new List<LeaveRequest> { r1, r2 }));

        var handler = new GetLeaveRequestsByEmployeeHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetLeaveRequestsByEmployeeQuery(e1.Value), CancellationToken.None);

        res.IsSuccess.Should().BeTrue(res.Error);
        res.Value!.Count.Should().Be(2);
        res.Value!.Should().OnlyContain(x => x.EmployeeId == e1.Value);
    }

    [Fact] // Unit
    public async Task Should_Propagate_Failure_From_Repository()
    {
        var repo = new Mock<LeaveRequestRepo>();
        var logger = new Mock<ILogger<GetLeaveRequestsByEmployeeHandler>>();
        var e1 = new EmployeeId(Guid.NewGuid());

        repo.Setup(r => r.GetByEmployeeAsync(e1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<LeaveRequest>>.Failure("boom"));

        var handler = new GetLeaveRequestsByEmployeeHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetLeaveRequestsByEmployeeQuery(e1.Value), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Be("boom");
    }
}

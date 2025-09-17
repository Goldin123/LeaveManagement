using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

using LeaveMgmt.Application.Queries.LeaveTypes;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveTypeRepo = LeaveMgmt.Application.Abstractions.Repositories.ILeaveTypeRepository;

public sealed class GetAllLeaveTypesTests
{
    [Fact] // Unit
    public async Task Should_Return_All_Types()
    {
        var repo = new Mock<LeaveTypeRepo>();
        var logger = new Mock<ILogger<GetAllLeaveTypesHandler>>();

        var items = new List<LeaveType>
        {
            new LeaveType("Annual", 30),
            new LeaveType("Sick", 10)
        };

        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<LeaveType>>.Success(items));

        var handler = new GetAllLeaveTypesHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetAllLeaveTypesQuery(), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value.Should().HaveCount(2);
    }

    [Fact] // Unit
    public async Task Should_Propagate_Failure()
    {
        var repo = new Mock<LeaveTypeRepo>();
        var logger = new Mock<ILogger<GetAllLeaveTypesHandler>>();

        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<LeaveType>>.Failure("boom"));

        var handler = new GetAllLeaveTypesHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetAllLeaveTypesQuery(), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Be("boom");
    }
}

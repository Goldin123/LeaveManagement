using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

using LeaveMgmt.Application.Queries.Holidays;
using LeaveMgmt.Domain.Holidays;
using LeaveMgmt.Domain;

using HolidayRepo = LeaveMgmt.Application.Abstractions.Repositories.IHolidayRepository;

public sealed class GetHolidaysTests
{
    [Fact] // Unit
    public async Task Should_Return_Holidays()
    {
        var repo = new Mock<HolidayRepo>();
        var logger = new Mock<ILogger<GetHolidaysHandler>>();

        var items = new List<Holiday>
{
    new Holiday("New Year", new DateTime(2025, 1, 1)),
    new Holiday("Christmas", new DateTime(2025, 12, 25))
};


        repo.Setup(r => r.GetHolidaysAsync(2025, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var handler = new GetHolidaysHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetHolidaysQuery(2025), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value.Should().HaveCount(2);
    }

    [Fact] // Unit
    public async Task Should_Handle_Exception()
    {
        var repo = new Mock<HolidayRepo>();
        var logger = new Mock<ILogger<GetHolidaysHandler>>();

        repo.Setup(r => r.GetHolidaysAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));

        var handler = new GetHolidaysHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetHolidaysQuery(2025), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Contain("fail");
    }
}

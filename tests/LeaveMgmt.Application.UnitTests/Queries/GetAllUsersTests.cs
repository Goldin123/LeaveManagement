using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

using LeaveMgmt.Application.Queries.Users;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using UserRepo = LeaveMgmt.Application.Abstractions.Repositories.IUserRepository;

public sealed class GetAllUsersTests
{
    [Fact] // Unit
    public async Task Should_Return_All_Users()
    {
        var repo = new Mock<UserRepo>();
        var logger = new Mock<ILogger<GetAllUsersHandler>>();

        var domainUsers = new List<User>
        {
            new User("alice@acme.com", "Alice", "hash1", "salt1", new[] { "Employee" }),
            new User("bob@acme.com", "Bob", "hash2", "salt2", new[] { "Manager" })
        };

        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<User>>.Success(domainUsers));

        var handler = new GetAllUsersHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        res.Value.Should().HaveCount(2);
        res.Value![0].Email.Should().Be("alice@acme.com");
    }

    [Fact] // Unit
    public async Task Should_Propagate_Failure()
    {
        var repo = new Mock<UserRepo>();
        var logger = new Mock<ILogger<GetAllUsersHandler>>();

        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<User>>.Failure("boom"));

        var handler = new GetAllUsersHandler(repo.Object, logger.Object);

        var res = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Be("boom");
    }
}

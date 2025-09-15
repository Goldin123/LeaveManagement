using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Application.Commands.Auth.RegisterUser;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using Xunit;

namespace LeaveMgmt.Application.UnitTests.Auth;

public class RegisterUserHandlerTests
{
    [Fact] // Unit
    public async Task Register_Should_Fail_When_Email_Not_In_Roster()
    {
        var users = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenService>();
        var roster = new Mock<ITeamRoster>();

        // Roster says "nope"
        IReadOnlyCollection<string> none = Array.Empty<string>();
        roster.Setup(r => r.TryGetRolesFor("nope@acme.com", out none)).Returns(false);

        var sut = new RegisterUserHandler(users.Object, hasher.Object, jwt.Object, roster.Object);

        var res = await sut.Handle(new RegisterUserCommand("nope@acme.com", "Nope User", "pw"), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Contain("not authorized");
    }

    [Fact] // Unit
    public async Task Register_Should_Create_User_And_Return_JWT()
    {
        var users = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenService>();
        var roster = new Mock<ITeamRoster>();

        // Roster hit
        IReadOnlyCollection<string> roles = new[] { "Employee" };
        roster.Setup(r => r.TryGetRolesFor("dev@acme.com", out roles)).Returns(true);

        // No existing user
        users.Setup(u => u.GetByEmailAsync("dev@acme.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<User?>.Success(null));

        // Hash + token
        hasher.Setup(h => h.Hash("pw")).Returns(("hash", "salt"));
        jwt.Setup(j => j.CreateToken(It.IsAny<Guid>(), "dev@acme.com", roles, null))
           .Returns("jwt-token");

        // Create success
        users.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<User>.Success(new User("dev@acme.com", "Dev User", "hash", "salt", roles)));

        var sut = new RegisterUserHandler(users.Object, hasher.Object, jwt.Object, roster.Object);

        var res = await sut.Handle(new RegisterUserCommand("dev@acme.com", "Dev User", "pw"), CancellationToken.None);

        res.IsSuccess.Should().BeTrue(res.Error);
        res.Value.Should().Be("jwt-token");
    }
}

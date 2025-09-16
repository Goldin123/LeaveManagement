using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.Abstractions.Security;
using LeaveMgmt.Application.Commands.Users.Login; // keep your actual namespace for LoginUserCommand
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Users;
using Xunit;

namespace LeaveMgmt.Application.UnitTests.Auth;

public class LoginUserHandlerTests
{
    [Fact] // Unit
    public async Task Login_Should_Fail_For_Unknown_Email()
    {
        var users = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenService>();

        users.Setup(u => u.GetByEmailAsync("nope@acme.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<User?>.Success(null));

        var sut = new LoginUserHandler(users.Object, hasher.Object, jwt.Object);

        var res = await sut.Handle(new LoginUserCommand("nope@acme.com", "pw"), CancellationToken.None);

        res.IsSuccess.Should().BeFalse();
        res.Error.Should().Contain("Invalid credentials");
    }

    [Fact] // Unit
    public async Task Login_Should_Return_Token_When_Password_Correct()
    {
        var users = new Mock<IUserRepository>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtTokenService>();

        // domain user
        var u = new User("dev@acme.com", "Dev User", "hash", "salt", new[] { "Employee" });

        users.Setup(r => r.GetByEmailAsync("dev@acme.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<User?>.Success(u));

        hasher.Setup(h => h.Verify("pw", "hash", "salt")).Returns(true);

        // FIX: match the actual values
        jwt.Setup(j => j.CreateToken(u.Id, u.Email, u.FullName, u.Roles, null))
           .Returns("jwt-token");

        var sut = new LoginUserHandler(users.Object, hasher.Object, jwt.Object);

        var res = await sut.Handle(new LoginUserCommand("dev@acme.com", "pw"), CancellationToken.None);

        res.IsSuccess.Should().BeTrue(res.Error);
        res.Value.Should().Be("jwt-token");
    }
}

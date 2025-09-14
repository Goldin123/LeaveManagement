using FluentValidation;
using LeaveMgmt.Application.Commands.Users.Register;
using LeaveMgmt.Application.Commands.Users.Login;

namespace LeaveMgmt.Application.Validation;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

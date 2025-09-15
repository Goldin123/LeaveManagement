using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.Users.Login; // keep your actual namespace
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Auth;

public sealed class LoginEndpoint(IMediator mediator) : Endpoint<LoginBody>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Summary(s => s.Summary = "Login with email/password and receive a JWT");
    }

    public override async Task HandleAsync(LoginBody req, CancellationToken ct)
    {
        var cmd = new LoginUserCommand(req.Email, req.Password);
        var res = await mediator.Send<Result<string>>(cmd, ct);  // returns JWT string

        if (res.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync<object>(
                new { token = res.Value },
                options: null,
                cancellationToken: ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync<object>(
                new { error = res.Error },
                options: null,
                cancellationToken: ct);
        }
    }
}

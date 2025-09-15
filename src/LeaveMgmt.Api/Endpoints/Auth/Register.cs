using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.Auth.RegisterUser;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Auth;

public sealed class RegisterEndpoint(IMediator mediator) : Endpoint<RegisterBody>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Summary(s => s.Summary = "Register only if your email is on the team roster (Dev/Management/Support).");
    }

    public override async Task HandleAsync(RegisterBody req, CancellationToken ct)
    {
        var cmd = new RegisterUserCommand(req.Email, req.FullName, req.Password);
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
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync<object>(
                new { error = res.Error },
                options: null,
                cancellationToken: ct);
        }
    }
}

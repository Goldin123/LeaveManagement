using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.Users.Register;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Auth;

public sealed class RegisterEndpoint(IMediator mediator)
    : Endpoint<RegisterRequest>
{
    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Summary(s => s.Summary = "Register a new user (default role: Employee)");
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var cmd = new RegisterUserCommand(req.Email, req.FullName, req.Password, null);
        var res = await mediator.Send<Result<Guid>>(cmd, ct);

        if (res.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(new { id = res.Value }, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = res.Error }, ct);
        }
    }
}

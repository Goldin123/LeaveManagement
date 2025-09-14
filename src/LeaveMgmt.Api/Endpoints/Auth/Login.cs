using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.Users.Login;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Auth;

public sealed class LoginEndpoint(IMediator mediator)
    : Endpoint<LoginRequest>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Summary(s => s.Summary = "Login and get JWT");
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var res = await mediator.Send<Result<string>>(new LoginUserCommand(req.Email, req.Password), ct);

        if (res.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(new LoginResponse(res.Value!), ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await HttpContext.Response.WriteAsJsonAsync(new { error = res.Error }, ct);
        }
    }
}

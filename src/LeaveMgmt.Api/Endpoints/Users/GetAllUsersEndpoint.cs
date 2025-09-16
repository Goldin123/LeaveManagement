using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Application.Queries.Users;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Users;

public sealed class GetAllUsersEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/users");
        Roles("Manager", "Admin");
        Summary(s => s.Summary = "Get all users (Manager/Admin only)");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<UserDto>>>(new GetAllUsersQuery(), ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(result.Value, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error }, ct);
        }
    }
}

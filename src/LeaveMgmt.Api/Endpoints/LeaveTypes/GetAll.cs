// File: LeaveMgmt.Api/Endpoints/LeaveTypes/GetAllLeaveTypesEndpoint.cs
using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Queries.LeaveTypes;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveTypes;

public sealed class GetAllLeaveTypesEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/leave-types");
        Roles("Employee", "Manager", "Admin"); // everyone logged in can see
        Summary(s => s.Summary = "Get all leave types");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<Domain.LeaveTypes.LeaveType>>>(new GetAllLeaveTypesQuery(), ct);

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

using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class RetractEndpoint(IMediator mediator)
    : Endpoint<(LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.RetractBody Body)>
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/retract");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "Retract a leave request by the owner");
    }

    public override async Task HandleAsync((LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.RetractBody Body) req, CancellationToken ct)
    {
        var result = await mediator.Send<Result>(new RetractLeaveCommand(req.Route.Id, req.Body.EmployeeId), ct);

        HttpContext.Response.StatusCode = result.IsSuccess
            ? StatusCodes.Status204NoContent
            : StatusCodes.Status400BadRequest;

        if (!result.IsSuccess)
            await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error }, ct);
    }
}

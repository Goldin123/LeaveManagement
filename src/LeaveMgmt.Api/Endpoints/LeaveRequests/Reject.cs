using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class RejectEndpoint(IMediator mediator)
    : Endpoint<(LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.RejectBody Body)>
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/reject");
        Roles("Manager", "Admin");
        Summary(s => s.Summary = "Reject a leave request");
    }

    public override async Task HandleAsync((LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.RejectBody Body) req, CancellationToken ct)
    {
        var result = await mediator.Send<Result>(new RejectLeaveCommand(req.Route.Id, req.Body.ManagerId, req.Body.Reason), ct);

        HttpContext.Response.StatusCode = result.IsSuccess
            ? StatusCodes.Status204NoContent
            : StatusCodes.Status400BadRequest;

        if (!result.IsSuccess)
            await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error }, ct);
    }
}

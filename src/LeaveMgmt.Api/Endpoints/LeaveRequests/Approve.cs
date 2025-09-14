using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class ApproveEndpoint(IMediator mediator)
    : Endpoint<(LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.ApproveBody Body)>
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/approve");
        Roles("Manager", "Admin");
        Summary(s => s.Summary = "Approve a leave request");
    }

    public override async Task HandleAsync((LeaveMgmt.Api.Endpoints.IdRoute Route, LeaveMgmt.Api.Endpoints.ApproveBody Body) req, CancellationToken ct)
    {
        var result = await mediator.Send<Result>(new ApproveLeaveCommand(req.Route.Id, req.Body.ManagerId), ct);

        HttpContext.Response.StatusCode = result.IsSuccess
            ? StatusCodes.Status204NoContent
            : StatusCodes.Status400BadRequest;

        if (!result.IsSuccess)
            await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error }, ct);
    }
}

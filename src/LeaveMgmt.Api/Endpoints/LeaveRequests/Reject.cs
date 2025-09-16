using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class RejectEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/reject");
        Roles("Manager", "Admin");
        Summary(s => s.Summary = "Reject a leave request");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id"); // route binding
        var body = await HttpContext.Request.ReadFromJsonAsync<RejectBody>(ct);

        if (body is null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new RejectResponse(false, "Missing body"), ct);
            return;
        }

        var cmd = new RejectLeaveCommand(id, body.ManagerId, body.Reason);
        var result = await mediator.Send<Result>(cmd, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new RejectResponse(true, null), ct);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new RejectResponse(false, result.Error), ct);
        }
    }
}

public record RejectBody(Guid ManagerId, string Reason);
public record RejectResponse(bool Success, string? Error);

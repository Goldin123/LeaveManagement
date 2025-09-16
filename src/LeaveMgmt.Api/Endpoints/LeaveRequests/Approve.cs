using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class ApproveEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/approve");
        Roles("Manager", "Admin");
        Summary(s => s.Summary = "Approve a leave request");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id"); // route binding
        var body = await HttpContext.Request.ReadFromJsonAsync<ApproveBody>(ct);

        if (body is null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ApproveResponse(false, "Missing body"), ct);
            return;
        }

        var cmd = new ApproveLeaveCommand(id, body.ManagerId);
        var result = await mediator.Send<Result>(cmd, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new ApproveResponse(true, null), ct);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ApproveResponse(false, result.Error), ct);
        }
    }
}

public record ApproveBody(Guid ManagerId);
public record ApproveResponse(bool Success, string? Error);

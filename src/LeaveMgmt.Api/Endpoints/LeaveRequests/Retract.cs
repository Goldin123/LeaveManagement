using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class RetractEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/leave-requests/{Id:guid}/retract");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "Retract a leave request by the owner");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var body = await HttpContext.Request.ReadFromJsonAsync<RetractBody>(ct);

        if (body is null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new RetractResponse(false, "Missing body"), ct);
            return;
        }

        var cmd = new RetractLeaveCommand(id, body.EmployeeId);
        var result = await mediator.Send<Result>(cmd, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new RetractResponse(true, null), ct);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new RetractResponse(false, result.Error), ct);
        }
    }
}

public record RetractBody(Guid EmployeeId);
public record RetractResponse(bool Success, string? Error);

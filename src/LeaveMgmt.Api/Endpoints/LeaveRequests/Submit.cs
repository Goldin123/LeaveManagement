using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class SubmitEndpoint(IMediator mediator)
    : Endpoint<LeaveMgmt.Api.Endpoints.SubmitLeaveRequestBody>
{
    public override void Configure()
    {
        Post("/leave-requests");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "Submit a leave request");
    }

    public override async Task HandleAsync(LeaveMgmt.Api.Endpoints.SubmitLeaveRequestBody req, CancellationToken ct)
    {
        var cmd = new SubmitLeaveRequestCommand(req.EmployeeId, req.LeaveTypeId, req.StartDate, req.EndDate, req.Reason);
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

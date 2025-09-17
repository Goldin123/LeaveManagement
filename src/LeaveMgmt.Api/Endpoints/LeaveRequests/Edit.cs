// File: Api/Endpoints/LeaveRequests/Edit.cs
using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Commands.LeaveRequests.EditLeave;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class EditEndpoint(IMediator mediator) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Put("/leave-requests/{Id:guid}/edit");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "Edit an existing leave request by the owner");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("Id");
        var body = await HttpContext.Request.ReadFromJsonAsync<EditBody>(ct);

        if (body is null)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new EditResponse(false, "Missing body"), ct);
            return;
        }

        var cmd = new EditLeaveCommand(
            id,
            body.EmployeeId,
            body.LeaveTypeId,
            body.StartDate,
            body.EndDate,
            body.Reason
        );

        var result = await mediator.Send<Result>(cmd, ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = 200;
            await HttpContext.Response.WriteAsJsonAsync(new EditResponse(true, null), ct);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new EditResponse(false, result.Error), ct);
        }
    }
}

public record EditBody(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason
);

public record EditResponse(bool Success, string? Error);

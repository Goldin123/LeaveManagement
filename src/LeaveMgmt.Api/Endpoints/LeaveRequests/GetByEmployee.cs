using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Application.Queries.LeaveRequests.GetByEmployee;
using LeaveMgmt.Domain;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.LeaveRequests;

public sealed class GetByEmployeeEndpoint(IMediator mediator)
    : Endpoint<LeaveMgmt.Api.Endpoints.ByEmployeeRoute>
{
    public override void Configure()
    {
        Get("/leave-requests/by-employee/{EmployeeId:guid}");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "List leave requests for an employee");
    }

    public override async Task HandleAsync(LeaveMgmt.Api.Endpoints.ByEmployeeRoute req, CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<LeaveRequestDto>>>(
            new GetLeaveRequestsByEmployeeQuery(req.EmployeeId), ct);

        if (result.IsSuccess)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(result.Value!, ct);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = result.Error }, ct);
        }
    }
}

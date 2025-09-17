using FastEndpoints;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Queries.Holidays;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Holidays;
using Microsoft.AspNetCore.Http;

namespace LeaveMgmt.Api.Endpoints.Holidays;

public sealed class GetHolidaysEndpoint(IMediator mediator) : Endpoint<GetHolidaysRequest>
{
    public override void Configure()
    {
        Get("/holidays/{Year:int}");
        Roles("Employee", "Manager", "Admin");
        Summary(s => s.Summary = "List public holidays for a given year");
    }

    public override async Task HandleAsync(GetHolidaysRequest req, CancellationToken ct)
    {
        var result = await mediator.Send<Result<IReadOnlyList<Holiday>>>(new GetHolidaysQuery(req.Year), ct);

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

public sealed class GetHolidaysRequest
{
    public int Year { get; set; }
}

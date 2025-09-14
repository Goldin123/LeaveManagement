using LeaveMgmt.Application;
using LeaveMgmt.Application.Abstractions;
//using LeaveMgmt.Application.Features.LeaveRequests;
using LeaveMgmt.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Layers
//builder.Services.AddApplication()
                //.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer(); // built-in (no Swagger UI added)

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

/*
app.MapPost("/leaves", async (CreateLeaveRequest body, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(body, ct);
    return result.IsSuccess
        ? Results.Created($"/leaves/{result.Value}", new { id = result.Value })
        : Results.BadRequest(new { error = result.Error });
});
*/
app.Run();

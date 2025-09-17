using LeaveMgmt.Api.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logPath = builder.Configuration["Serilog:Path"]
              ?? "Logs/log-.txt"; // fallback if missing

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // less noise
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Information()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();
app.UseApi();

app.Run();

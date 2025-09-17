using LeaveMgmt.Api.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logPath = builder.Configuration["Serilog:Path"]
              ?? "Logs/log-.txt"; // fallback if missing

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();
app.UseApi();

app.Run();

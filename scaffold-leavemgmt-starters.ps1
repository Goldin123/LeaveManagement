param(
  [string]$Name = "LeaveMgmt",
  [string]$Path = "D:\Sandbox\ASP"
)

$root = Join-Path $Path $Name
$src  = Join-Path $root "src"

$domProj = Join-Path $src "$Name.Domain"
$appProj = Join-Path $src "$Name.Application"
$infProj = Join-Path $src "$Name.Infrastructure"
$apiProj = Join-Path $src "$Name.Api"

function WriteFile($path, $content) {
    New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
    $content | Set-Content -Path $path -Encoding UTF8
}

# ============== Domain =====================

# Result.cs
$domainResult = @"
namespace $Name.Domain;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool ok, string? error) { IsSuccess = ok; Error = error; }
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Value { get; }

    private Result(bool ok, T? value, string? error) { IsSuccess = ok; Value = value; Error = error; }
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);

    public void Deconstruct(out bool ok, out T? value, out string? error)
        => (ok, value, error) = (IsSuccess, Value, Error);
}
"@
WriteFile (Join-Path $domProj "Abstractions\Result.cs") $domainResult

# DateRange value object
$dateRange = @"
namespace $Name.Domain.ValueObjects;

public readonly struct DateRange
{
    public DateOnly From { get; }
    public DateOnly To   { get; }
    public int Days => To.DayNumber - From.DayNumber + 1;

    public DateRange(DateOnly from, DateOnly to)
    {
        if (to < from) throw new ArgumentException(""To must be >= From"");
        From = from; To = to;
    }

    public bool Overlaps(DateRange other) =>
        From <= other.To && other.From <= To;
}
"@
WriteFile (Join-Path $domProj "ValueObjects\DateRange.cs") $dateRange

# Entities
$entities = @"
namespace $Name.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
}

public enum LeaveType { Annual, Sick, Unpaid }

public class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To   { get; set; }
    public string Status { get; set; } = ""Pending"";
}
"@
WriteFile (Join-Path $domProj "Entities\Entities.cs") $entities

# ============== Application =====================

# Mediator abstractions + implementation
$mediator = @"
namespace $Name.Application.Abstractions;

// ----------- Contracts -----------
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest,TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}

// ----------- Implementation -----------
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    public Mediator(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _sp.GetService(handlerType)
            ?? throw new InvalidOperationException($""No handler registered for {request.GetType().Name}"");

        var method = handlerType.GetMethod(""Handle"")!;
        return (Task<TResponse>)method.Invoke(handler, new object?[] { request, ct })!;
    }
}
"@
WriteFile (Join-Path $appProj "Abstractions\Mediator.cs") $mediator

# Common interfaces (time, repos)
$appAbstractions = @"
namespace $Name.Application.Abstractions;

public interface IDateTime
{
    DateTime UtcNow { get; }
}

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    Task UpdateAsync(T entity, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
"@
WriteFile (Join-Path $appProj "Abstractions\Common.cs") $appAbstractions

# DTOs
$dtos = @"
namespace $Name.Application.DTOs;

public sealed class LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateOnly From { get; init; }
    public DateOnly To   { get; init; }
    public string Status { get; init; } = ""Pending"";
}
"@
WriteFile (Join-Path $appProj "DTOs\LeaveRequestDto.cs") $dtos

# Feature: CreateLeaveRequest (Command + Handler) — uses IRepository<LeaveRequest>
$featureCreate = @"
using $Name.Application.Abstractions;
using $Name.Domain;
using $Name.Domain.Entities;

namespace $Name.Application.Features.LeaveRequests;

public sealed record CreateLeaveRequest(
    Guid EmployeeId,
    string Type,
    DateOnly From,
    DateOnly To
) : IRequest<Result<Guid>>;

public sealed class CreateLeaveRequestHandler : IRequestHandler<CreateLeaveRequest, Result<Guid>>
{
    private readonly IRepository<LeaveRequest> _repo;

    public CreateLeaveRequestHandler(IRepository<LeaveRequest> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LeaveType>(request.Type, ignoreCase: true, out var parsed))
            return Result<Guid>.Failure($""Unknown leave type '{request.Type}'."");

        var entity = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = parsed,
            From = request.From,
            To   = request.To,
            Status = ""Pending""
        };

        await _repo.AddAsync(entity, ct);
        return Result<Guid>.Success(entity.Id);
    }
}
"@
WriteFile (Join-Path $appProj "Features\LeaveRequests\CreateLeaveRequest.cs") $featureCreate

# Application DependencyInjection
$appDi = @"
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using $Name.Application.Abstractions;

namespace $Name.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        // Auto-register all IRequestHandler<,> found in this assembly
        var asm = Assembly.GetExecutingAssembly();
        var handlerInterface = typeof(IRequestHandler<,>);
        foreach (var t in asm.GetTypes())
        {
            var hi = t.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            if (hi is not null && t is { IsAbstract: false, IsInterface: false })
            {
                services.AddTransient(hi, t);
            }
        }

        return services;
    }
}
"@
WriteFile (Join-Path $appProj "DependencyInjection.cs") $appDi

# ============== Infrastructure =====================

# In-memory DateTime + Repo implementations
$infraServices = @"
using $Name.Application.Abstractions;
using $Name.Domain.Entities;

namespace $Name.Infrastructure.Services;

public sealed class SystemClock : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<Guid,T> _store = new();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

    public Task AddAsync(T entity, CancellationToken ct)
    {
        var idProp = typeof(T).GetProperty(""Id"");
        if (idProp is null || idProp.PropertyType != typeof(Guid))
            throw new InvalidOperationException($""Type {typeof(T).Name} must have Guid Id property"");

        var id = (Guid) (idProp.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty) { id = Guid.NewGuid(); idProp.SetValue(entity, id); }
        _store[id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken ct)
    {
        var idProp = typeof(T).GetProperty(""Id"") ?? throw new InvalidOperationException(""Missing Id"");
        var id = (Guid)(idProp.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty) throw new InvalidOperationException(""Id cannot be empty"");
        _store[id] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }
}
"@
WriteFile (Join-Path $infProj "Persistence\InMemory.cs") $infraServices

# Infrastructure DI
$infraDi = @"
using Microsoft.Extensions.DependencyInjection;
using $Name.Application.Abstractions;
using $Name.Domain.Entities;
using $Name.Infrastructure.Services;

namespace $Name.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, SystemClock>();

        // repositories
        services.AddSingleton<IRepository<LeaveRequest>, InMemoryRepository<LeaveRequest>>();

        return services;
    }
}
"@
WriteFile (Join-Path $infProj "DependencyInjection.cs") $infraDi

# ============== API (top-level Program.cs) =====================

$program = @"
using $Name.Application;
using $Name.Application.Abstractions;
using $Name.Application.Features.LeaveRequests;
using $Name.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplication()
                .AddInfrastructure();

builder.Services.AddEndpointsApiExplorer(); // built-in (no Swagger UI added)

var app = builder.Build();

app.MapGet(""/health"", () => Results.Ok(new { status = ""ok"", time = DateTime.UtcNow }));

app.MapPost(""/leaves"", async (CreateLeaveRequest body, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(body, ct);
    return result.IsSuccess
        ? Results.Created($""/leaves/{result.Value}"", new { id = result.Value })
        : Results.BadRequest(new { error = result.Error });
});

app.Run();
"@
WriteFile (Join-Path $apiProj "Program.cs") $program

Write-Host "✅ Starter code scaffolded."
Write-Host "   Domain -> Result, Entities, ValueObjects"
Write-Host "   Application -> Minimal Mediator, Command/Handler, DI"
Write-Host "   Infrastructure -> InMemory repo + DI"
Write-Host "   Api -> Top-level Program.cs with /health and POST /leaves"

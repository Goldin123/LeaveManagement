// File: LeaveMgmt.Application/Behaviors/ValidationBehavior.cs
using FluentValidation;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Application.Abstractions;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResponse>> next)
    {
        if (!validators.Any()) return await next();

        var ctx = new ValidationContext<TRequest>(request);
        var failures = new List<string>();

        foreach (var v in validators)
        {
            var res = await v.ValidateAsync(ctx, cancellationToken);
            if (!res.IsValid)
                failures.AddRange(res.Errors.Select(e => e.ErrorMessage));
        }

        if (failures.Count == 0) return await next();

        // Expecting Result / Result<T> return type from handlers
        var t = typeof(TResponse);
        if (t == typeof(Result))
            return (TResponse)(object)Result.Failure(string.Join("; ", failures));

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var ctor = t.GetMethod("Failure", [typeof(string)])!;
            return (TResponse)ctor.Invoke(null, new object[] { string.Join("; ", failures) })!;
        }

        // If a handler returns a non-Result type, let it proceed (or throw).
        return await next();
    }
}

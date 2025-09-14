namespace LeaveMgmt.Application.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

// ----------- Contracts -----------
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}


// ----------- Mediator with behaviors -----------
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    public Mediator(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();

        // Resolve the handler: IRequestHandler<TRequest,TResponse>
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _sp.GetRequiredService(handlerType);

        // Resolve all behaviors: IEnumerable<IPipelineBehavior<TRequest,TResponse>>
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _sp.GetServices(behaviorType).ToList();

        // Terminal delegate that invokes the handler
        Task<TResponse> InvokeHandler()
            => (Task<TResponse>)handlerType
                .GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!
                .Invoke(handler, new object?[] { request, ct })!;

        // Fold behaviors in reverse order so they wrap around the handler
        Func<Task<TResponse>> next = InvokeHandler;
        for (int i = behaviors.Count - 1; i >= 0; i--)
        {
            var b = behaviors[i];
            var method = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;
            var currentNext = next;
            next = () => (Task<TResponse>)method.Invoke(b, new object?[] { request, ct, currentNext })!;
        }

        return next();
    }
}

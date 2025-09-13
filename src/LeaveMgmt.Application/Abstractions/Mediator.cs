

namespace LeaveMgmt.Application.Abstractions;

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



public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _sp;
    public Mediator(IServiceProvider sp) => _sp = sp;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = _sp.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");

        var method = handlerType.GetMethod("Handle")!;
        return (Task<TResponse>)method.Invoke(handler, new object?[] { request, ct })!;
    }
}

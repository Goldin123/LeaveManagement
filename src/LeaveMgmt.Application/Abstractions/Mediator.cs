using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeaveMgmt.Application.Abstractions
{
    public interface IRequest<out TResponse> { }

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request, CancellationToken ct);
    }

    //public interface IPipelineBehavior<in TRequest, TResponse>
    //    where TRequest : IRequest<TResponse>
    //{
    //    Task<TResponse> Handle(TRequest request, CancellationToken ct, Func<Task<TResponse>> next);
    //}

    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    }

    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _sp;
        public Mediator(IServiceProvider sp) => _sp = sp;

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
        {
            using var scope = _sp.CreateScope(); // keep it alive until after handler finishes
            var provider = scope.ServiceProvider;

            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = provider.GetRequiredService(handlerType);

            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var behaviors = provider.GetServices(behaviorType).ToList();

            Task<TResponse> InvokeHandler()
                => (Task<TResponse>)handlerType
                    .GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!
                    .Invoke(handler, new object?[] { request, ct })!;

            Func<Task<TResponse>> next = InvokeHandler;
            for (int i = behaviors.Count - 1; i >= 0; i--)
            {
                var b = behaviors[i];
                var method = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!;
                var currentNext = next;
                next = () => (Task<TResponse>)method.Invoke(b, new object?[] { request, ct, currentNext })!;
            }

            return await next(); // scope disposed only after this finishes
        }
    }
}

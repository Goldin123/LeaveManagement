// File: LeaveMgmt.Application/Abstractions/IPipelineBehavior.cs
namespace LeaveMgmt.Application.Abstractions;

public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResponse>> next);
}

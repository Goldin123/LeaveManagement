using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;
using LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;
using LeaveMgmt.Application.Common;
using LeaveMgmt.Domain;

namespace LeaveMgmt.Application.Abstractions;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        Func<Task<TResponse>> next)
    {
        string? error = null;

        if (!currentUser.IsAuthenticated)
        {
            error = "Unauthenticated.";
        }
        else
        {
            // Basic policy:
            // - Submit: caller must be the employee
            // - Retract: caller must be the owner
            // - Approve/Reject: caller must be in Manager (or Admin) role
            switch (request)
            {
                case SubmitLeaveRequestCommand c when c.EmployeeId != currentUser.UserId:
                    error = "You can only submit your own leave.";
                    break;

                case RetractLeaveCommand c when c.EmployeeId != currentUser.UserId:
                    error = "You can only retract your own leave.";
                    break;

                case ApproveLeaveCommand or RejectLeaveCommand
                    when !(currentUser.IsInRole(Roles.Manager) || currentUser.IsInRole(Roles.Admin)):
                    error = "Manager permissions required.";
                    break;
            }
        }

        if (error is null) return next();

        // Return Result/Result<T> failure consistently
        var t = typeof(TResponse);
        if (t == typeof(Result))
            return Task.FromResult((TResponse)(object)Result.Failure(error));

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failure = t.GetMethod("Failure", [typeof(string)])!;
            return Task.FromResult((TResponse)failure.Invoke(null, new object[] { error })!);
        }

        // Fallback: throw
        throw new UnauthorizedAccessException(error);
    }
}

using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;

public sealed record RejectLeaveCommand(Guid LeaveRequestId, Guid ManagerId, string Reason) : IRequest<Result>;

public sealed class RejectLeaveHandler(ILeaveRequestRepository repo)
    : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
                return Result.Failure(found.Error ?? "Leave request not found.");

            var rej = found.Value.Reject(new ManagerId(cmd.ManagerId), cmd.Reason); // domain method 
            if (!rej.IsSuccess) return Result.Failure(rej.Error!);

            return await repo.UpdateAsync(found.Value, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Reject failed: {ex.Message}");
        }
    }
}

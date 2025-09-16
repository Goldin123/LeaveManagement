using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;

public sealed record ApproveLeaveCommand(Guid LeaveRequestId, Guid ManagerId) : IRequest<Result>;

public sealed class ApproveLeaveHandler(ILeaveRequestRepository repo)
    : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
                return Result.Failure(found.Error ?? "Leave request not found.");

            var approve = found.Value.Approve(new ManagerId(cmd.ManagerId)); // domain method 
            if (!approve.IsSuccess) return Result.Failure(approve.Error!);

            return await repo.UpdateAsync(found.Value, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Approve failed: {ex.Message}");
        }
    }
}

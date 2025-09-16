using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;

public sealed record RetractLeaveCommand(Guid LeaveRequestId, Guid EmployeeId) : IRequest<Result>;

public sealed class RetractLeaveHandler(ILeaveRequestRepository repo)
    : IRequestHandler<RetractLeaveCommand, Result>
{
    public async Task<Result> Handle(RetractLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
                return Result.Failure(found.Error ?? "Leave request not found.");

            // Domain requires owner id to retract 
            var res = found.Value.Retract(new EmployeeId(cmd.EmployeeId));
            if (!res.IsSuccess) return Result.Failure(res.Error!);

            return await repo.UpdateAsync(found.Value, ct);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Retract failed: {ex.Message}");
        }
    }
}

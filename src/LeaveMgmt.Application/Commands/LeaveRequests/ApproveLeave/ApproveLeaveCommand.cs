using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.LeaveRequests.ApproveLeave;

/// <summary>
/// Command request: approve an existing leave request by a manager.
/// </summary>
public sealed record ApproveLeaveCommand(Guid LeaveRequestId, Guid ManagerId) : IRequest<Result>;

/// <summary>
/// Handles approval of leave requests. Uses repository for persistence
/// and logs each important step (attempt, validation, result).
/// </summary>
public sealed class ApproveLeaveHandler(ILeaveRequestRepository repo, ILogger<ApproveLeaveHandler> logger)
    : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation(
                "Manager {ManagerId} attempting to approve leave request {LeaveRequestId}",
                cmd.ManagerId, cmd.LeaveRequestId);

            // 1) Fetch the leave request from the repository
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
            {
                logger.LogWarning(
                    "Leave request {LeaveRequestId} not found or lookup failed. Error: {Error}",
                    cmd.LeaveRequestId, found.Error);
                return Result.Failure(found.Error ?? "Leave request not found.");
            }

            // 2) Perform domain-level approval validation
            var approve = found.Value.Approve(new ManagerId(cmd.ManagerId));
            if (!approve.IsSuccess)
            {
                logger.LogWarning(
                    "Approval domain validation failed for leave request {LeaveRequestId}: {Error}",
                    cmd.LeaveRequestId, approve.Error);
                return Result.Failure(approve.Error!);
            }

            // 3) Persist the approved leave request
            var result = await repo.UpdateAsync(found.Value, ct);
            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Leave request {LeaveRequestId} approved by manager {ManagerId}",
                    cmd.LeaveRequestId, cmd.ManagerId);
            }
            else
            {
                logger.LogError(
                    "Failed to persist approval for leave request {LeaveRequestId}: {Error}",
                    cmd.LeaveRequestId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex,
                "Unhandled exception while approving leave request {LeaveRequestId}",
                cmd.LeaveRequestId);
            return Result.Failure($"Approve failed: {ex.Message}");
        }
    }
}

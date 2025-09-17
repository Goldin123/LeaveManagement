using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.LeaveRequests.RejectLeave;

/// <summary>
/// Command request: reject an existing leave request with a reason.
/// </summary>
public sealed record RejectLeaveCommand(Guid LeaveRequestId, Guid ManagerId, string Reason) : IRequest<Result>;

/// <summary>
/// Handles rejection of leave requests. Performs domain validation,
/// persists the change, and logs each important step.
/// </summary>
public sealed class RejectLeaveHandler(ILeaveRequestRepository repo, ILogger<RejectLeaveHandler> logger)
    : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation(
                "Manager {ManagerId} attempting to reject leave request {LeaveRequestId} with reason: {Reason}",
                cmd.ManagerId, cmd.LeaveRequestId, cmd.Reason);

            // 1) Fetch the leave request from the repository
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
            {
                logger.LogWarning(
                    "Leave request {LeaveRequestId} not found or lookup failed. Error: {Error}",
                    cmd.LeaveRequestId, found.Error);
                return Result.Failure(found.Error ?? "Leave request not found.");
            }

            // 2) Perform domain-level rejection validation
            var rej = found.Value.Reject(new ManagerId(cmd.ManagerId), cmd.Reason);
            if (!rej.IsSuccess)
            {
                logger.LogWarning(
                    "Rejection domain validation failed for leave request {LeaveRequestId}: {Error}",
                    cmd.LeaveRequestId, rej.Error);
                return Result.Failure(rej.Error!);
            }

            // 3) Persist the rejected leave request
            var result = await repo.UpdateAsync(found.Value, ct);
            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Leave request {LeaveRequestId} rejected by manager {ManagerId} with reason: {Reason}",
                    cmd.LeaveRequestId, cmd.ManagerId, cmd.Reason);
            }
            else
            {
                logger.LogError(
                    "Failed to persist rejection for leave request {LeaveRequestId}: {Error}",
                    cmd.LeaveRequestId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex,
                "Unhandled exception while rejecting leave request {LeaveRequestId}",
                cmd.LeaveRequestId);
            return Result.Failure($"Reject failed: {ex.Message}");
        }
    }
}

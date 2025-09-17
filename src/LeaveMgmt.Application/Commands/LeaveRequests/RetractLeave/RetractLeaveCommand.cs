using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.LeaveRequests.RetractLeave;

/// <summary>
/// Command request: retract (cancel) an existing leave request by its owner.
/// </summary>
public sealed record RetractLeaveCommand(Guid LeaveRequestId, Guid EmployeeId) : IRequest<Result>;

/// <summary>
/// Handles retraction of leave requests. Ensures only the owner can retract,
/// persists the change, and logs each important step.
/// </summary>
public sealed class RetractLeaveHandler(ILeaveRequestRepository repo, ILogger<RetractLeaveHandler> logger)
    : IRequestHandler<RetractLeaveCommand, Result>
{
    public async Task<Result> Handle(RetractLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation(
                "Employee {EmployeeId} attempting to retract leave request {LeaveRequestId}",
                cmd.EmployeeId, cmd.LeaveRequestId);

            // 1) Fetch the leave request from the repository
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
            {
                logger.LogWarning(
                    "Leave request {LeaveRequestId} not found or lookup failed. Error: {Error}",
                    cmd.LeaveRequestId, found.Error);
                return Result.Failure(found.Error ?? "Leave request not found.");
            }

            // 2) Perform domain-level retraction validation (only owner can retract)
            var res = found.Value.Retract(new EmployeeId(cmd.EmployeeId));
            if (!res.IsSuccess)
            {
                logger.LogWarning(
                    "Retraction validation failed for leave request {LeaveRequestId} by employee {EmployeeId}: {Error}",
                    cmd.LeaveRequestId, cmd.EmployeeId, res.Error);
                return Result.Failure(res.Error!);
            }

            // 3) Persist the retracted leave request
            var result = await repo.UpdateAsync(found.Value, ct);
            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Leave request {LeaveRequestId} successfully retracted by employee {EmployeeId}",
                    cmd.LeaveRequestId, cmd.EmployeeId);
            }
            else
            {
                logger.LogError(
                    "Failed to persist retraction for leave request {LeaveRequestId}: {Error}",
                    cmd.LeaveRequestId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex,
                "Unhandled exception while retracting leave request {LeaveRequestId} by employee {EmployeeId}",
                cmd.LeaveRequestId, cmd.EmployeeId);
            return Result.Failure($"Retract failed: {ex.Message}");
        }
    }
}

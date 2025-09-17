// File: Application/Commands/LeaveRequests/EditLeave/EditLeaveCommand.cs
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.LeaveRequests.EditLeave;

public sealed record EditLeaveCommand(
    Guid LeaveRequestId,
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason
) : IRequest<Result>;

public sealed class EditLeaveHandler(ILeaveRequestRepository repo, ILogger<EditLeaveHandler> logger)
    : IRequestHandler<EditLeaveCommand, Result>
{
    public async Task<Result> Handle(EditLeaveCommand cmd, CancellationToken ct)
    {
        try
        {
            var found = await repo.GetByIdAsync(cmd.LeaveRequestId, ct);
            if (!found.IsSuccess || found.Value is null)
                return Result.Failure(found.Error ?? "Leave request not found.");

            var req = found.Value;
            if (req.EmployeeId.Value != cmd.EmployeeId)
                return Result.Failure("Only the owner can edit their request.");
            if (req.Status != Domain.LeaveRequests.LeaveStatus.Submitted &&
                req.Status != Domain.LeaveRequests.LeaveStatus.Draft)
                return Result.Failure("Only draft or submitted requests can be edited.");

            // Apply changes
            req.Edit(new EmployeeId(cmd.EmployeeId), cmd.LeaveTypeId,
                new DateRange(cmd.StartDate, cmd.EndDate), cmd.Reason);

            return await repo.UpdateAsync(req, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to edit leave request {LeaveRequestId}", cmd.LeaveRequestId);
            return Result.Failure($"Edit failed: {ex.Message}");
        }
    }
}

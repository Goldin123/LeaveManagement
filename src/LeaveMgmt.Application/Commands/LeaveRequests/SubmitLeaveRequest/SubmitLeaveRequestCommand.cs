using LeaveMgmt.Application.Abstractions; // IRequest, IRequestHandler
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;                    // Result, Result<T>
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;

/// <summary>
/// Command request: submit a new leave request.
/// </summary>
public sealed record SubmitLeaveRequestCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason) : IRequest<Result<Guid>>;

/// <summary>
/// Handles submission of leave requests. Performs validation on leave type,
/// constructs the aggregate, calls domain Submit, and persists the result.
/// Logs each important step.
/// </summary>
public sealed class SubmitLeaveRequestHandler(
    ILeaveRequestRepository leaveRequests,
    ILeaveTypeRepository leaveTypes,
    ILogger<SubmitLeaveRequestHandler> logger)
    : IRequestHandler<SubmitLeaveRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SubmitLeaveRequestCommand cmd, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation(
                "Employee {EmployeeId} submitting leave request for type {LeaveTypeId} from {StartDate} to {EndDate}",
                cmd.EmployeeId, cmd.LeaveTypeId, cmd.StartDate, cmd.EndDate);

            // 1) Validate leave type exists
            var ltRes = await leaveTypes.GetByIdAsync(cmd.LeaveTypeId, ct);
            if (!ltRes.IsSuccess || ltRes.Value is null)
            {
                logger.LogWarning(
                    "Leave type {LeaveTypeId} not found or failed lookup. Error: {Error}",
                    cmd.LeaveTypeId, ltRes.Error);
                return Result<Guid>.Failure(ltRes.Error ?? "Leave type not found.");
            }

            // 2) Construct domain aggregate (LeaveRequest)
            var employee = new EmployeeId(cmd.EmployeeId);
            var range = new DateRange(cmd.StartDate, cmd.EndDate);

            var req = new LeaveRequest(employee, ltRes.Value, range, cmd.Reason);

            // 3) Call domain Submit() method
            var submitted = req.Submit();
            if (!submitted.IsSuccess)
            {
                logger.LogWarning(
                    "Submission failed validation for employee {EmployeeId} leave request: {Error}",
                    cmd.EmployeeId, submitted.Error);
                return Result<Guid>.Failure(submitted.Error!);
            }

            // 4) Persist the new leave request
            var created = await leaveRequests.CreateAsync(req, ct);
            if (created.IsSuccess)
            {
                logger.LogInformation(
                    "Leave request {LeaveRequestId} successfully created for employee {EmployeeId}",
                    req.Id, cmd.EmployeeId);
                return Result<Guid>.Success(req.Id);
            }
            else
            {
                logger.LogError(
                    "Failed to persist leave request for employee {EmployeeId}: {Error}",
                    cmd.EmployeeId, created.Error);
                return Result<Guid>.Failure(created.Error!);
            }
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex,
                "Unhandled exception while submitting leave request for employee {EmployeeId}",
                cmd.EmployeeId);
            return Result<Guid>.Failure($"Submit failed: {ex.Message}");
        }
    }
}

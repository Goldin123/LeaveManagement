using LeaveMgmt.Application.Abstractions; // IRequest, IRequestHandler
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;                    // Result, Result<T>
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Commands.LeaveRequests.SubmitLeaveRequest;

public sealed record SubmitLeaveRequestCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason) : IRequest<Result<Guid>>;

public sealed class SubmitLeaveRequestHandler(
    ILeaveRequestRepository leaveRequests,
    ILeaveTypeRepository leaveTypes)
    : IRequestHandler<SubmitLeaveRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(SubmitLeaveRequestCommand cmd, CancellationToken ct)
    {
        try
        {
            var ltRes = await leaveTypes.GetByIdAsync(cmd.LeaveTypeId, ct);
            if (!ltRes.IsSuccess || ltRes.Value is null)
                return Result<Guid>.Failure(ltRes.Error ?? "Leave type not found.");

            var employee = new EmployeeId(cmd.EmployeeId);
            var range = new DateRange(cmd.StartDate, cmd.EndDate); // persisted via converter 

            // Domain aggregate uses ctor + Submit() 
            var req = new LeaveRequest(employee, ltRes.Value, range, cmd.Reason);
            var submitted = req.Submit();
            if (!submitted.IsSuccess) return Result<Guid>.Failure(submitted.Error!);           

            var created = await leaveRequests.CreateAsync(req, ct);
            return created.IsSuccess ? Result<Guid>.Success(req.Id) : Result<Guid>.Failure(created.Error!);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Submit failed: {ex.Message}");
        }
    }
}

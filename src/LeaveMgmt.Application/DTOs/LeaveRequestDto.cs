using LeaveMgmt.Domain.LeaveRequests;

namespace LeaveMgmt.Application.DTOs;

public sealed class LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public Guid LeaveTypeId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string Status { get; init; } = default!;
    public Guid? ApprovedBy { get; init; }
    public DateTime? DecidedUtc { get; init; }

    public static LeaveRequestDto FromDomain(LeaveRequest r) => new()
    {
        Id = r.Id,
        EmployeeId = r.EmployeeId.Value,
        LeaveTypeId = r.LeaveTypeId,
        StartDate = r.Range.Start,
        EndDate = r.Range.End,
        Status = r.Status.ToString(),
        ApprovedBy = r.ApprovedBy?.Value,
        DecidedUtc = r.DecidedUtc
    };
}

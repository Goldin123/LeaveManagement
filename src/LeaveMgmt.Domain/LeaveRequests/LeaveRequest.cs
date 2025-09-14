using LeaveMgmt.Domain.Common;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Domain.LeaveRequests;

public sealed class LeaveRequest : Entity
{
    public EmployeeId EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public DateRange Range { get; private set; }
    public string Reason { get; private set; }
    public LeaveStatus Status { get; private set; } = LeaveStatus.Draft;

    public ManagerId? ApprovedBy { get; private set; }
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? SubmittedUtc { get; private set; }
    public DateTime? DecidedUtc { get; private set; }

    private LeaveRequest() { } // EF

    public LeaveRequest(EmployeeId employeeId, LeaveType type, DateRange range, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("Reason is required.");
        EmployeeId = employeeId;
        LeaveTypeId = type.Id;
        Range = range;
        Reason = reason.Trim();

        if (range.Days > type.MaxDaysPerRequest)
            throw new DomainException($"Request exceeds max days ({type.MaxDaysPerRequest}).");
    }

    // Submit
    public DomainResult Submit()
    {
        if (Status != LeaveStatus.Draft)
            return DomainResult.Fail("Only draft requests can be submitted.");
        Status = LeaveStatus.Submitted;
        SubmittedUtc = DateTime.UtcNow;
        Raise(new Events.LeaveRequestSubmitted(Id));
        return DomainResult.Success();
    }

    // Approve
    public DomainResult Approve(ManagerId manager)
    {
        if (Status != LeaveStatus.Submitted)
            return DomainResult.Fail("Only submitted requests can be approved.");
        Status = LeaveStatus.Approved;
        ApprovedBy = manager;
        DecidedUtc = DateTime.UtcNow;
        Raise(new Events.LeaveRequestApproved(Id, manager.Value));
        return DomainResult.Success();
    }

    // Reject
    public DomainResult Reject(ManagerId manager, string reason)
    {
        if (Status != LeaveStatus.Submitted)
            return DomainResult.Fail("Only submitted requests can be rejected.");
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Fail("Rejection reason required.");
        Status = LeaveStatus.Rejected;
        ApprovedBy = manager;
        DecidedUtc = DateTime.UtcNow;
        Raise(new Events.LeaveRequestRejected(Id, manager.Value, reason.Trim()));
        return DomainResult.Success();
    }

    // Retract (by employee) – only if submitted
    public DomainResult Retract(EmployeeId by)
    {
        if (by.Value != EmployeeId.Value)
            return DomainResult.Fail("Only the owner can retract their request.");
        if (Status != LeaveStatus.Submitted)
            return DomainResult.Fail("Only submitted requests can be retracted.");
        Status = LeaveStatus.Retracted;
        DecidedUtc = DateTime.UtcNow;
        Raise(new Events.LeaveRequestRetracted(Id));
        return DomainResult.Success();
    }
}

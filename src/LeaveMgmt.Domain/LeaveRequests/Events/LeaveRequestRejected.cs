using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.LeaveRequests.Events;

public sealed class LeaveRequestRejected(Guid requestId, Guid managerId, string reason) : DomainEventBase
{
    public Guid RequestId { get; } = requestId;
    public Guid ManagerId { get; } = managerId;
    public string Reason { get; } = reason;
}

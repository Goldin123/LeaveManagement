using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.LeaveRequests.Events;

public sealed class LeaveRequestApproved(Guid requestId, Guid managerId) : DomainEventBase
{
    public Guid RequestId { get; } = requestId;
    public Guid ManagerId { get; } = managerId;
}

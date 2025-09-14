using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.LeaveRequests.Events;

public sealed class LeaveRequestRetracted(Guid requestId) : DomainEventBase
{
    public Guid RequestId { get; } = requestId;
}

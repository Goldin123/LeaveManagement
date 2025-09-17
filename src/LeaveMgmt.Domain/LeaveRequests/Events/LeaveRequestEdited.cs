// File: Domain/LeaveRequests/Events/LeaveRequestEdited.cs
using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.LeaveRequests.Events;

public sealed class LeaveRequestEdited(Guid requestId) : DomainEventBase
{
    public Guid RequestId { get; } = requestId;
}

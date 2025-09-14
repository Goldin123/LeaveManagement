using FluentAssertions;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;
using Xunit;

namespace LeaveMgmt.Domain.UnitTests.Events;

public class LeaveRequestEventsTests
{
    [Fact]
    public void Submit_ShouldRaise_SubmittedEvent()
    {
        var emp = new EmployeeId(Guid.NewGuid());
        var req = new LeaveRequest(emp, new("Annual", 10), new(new(2025, 2, 1), new(2025, 2, 2)), "Trip");

        req.Submit();

        req.DomainEvents.Should().ContainSingle(e => e.GetType().Name.Contains("Submitted"));
    }
}

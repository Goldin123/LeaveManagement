using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Domain.Services;

public static class LeavePolicy
{
    // Example rule hook: could check annual quota, blackout dates, etc.
    public static bool CanSubmit(LeaveRequest req, LeaveType type, IEnumerable<DateRange> employeeExistingRanges)
    {
        // no overlap with existing approved ranges
        if (employeeExistingRanges.Any(r => r.Overlaps(req.Range))) return false;
        // already validated per-type max days in ctor
        return true;
    }
}

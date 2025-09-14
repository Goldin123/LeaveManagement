using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.LeaveTypes;

public sealed class LeaveType : Entity
{
    public string Name { get; private set; } = default!;
    public int MaxDaysPerRequest { get; private set; }

    private LeaveType() { } // for EF
    public LeaveType(string name, int maxDaysPerRequest)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Leave type name is required.");
        if (maxDaysPerRequest <= 0) throw new DomainException("Max days must be > 0.");
        Name = name.Trim();
        MaxDaysPerRequest = maxDaysPerRequest;
    }
}

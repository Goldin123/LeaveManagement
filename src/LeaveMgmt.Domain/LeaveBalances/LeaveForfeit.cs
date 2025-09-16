namespace LeaveMgmt.Domain.LeaveBalances;

public sealed class LeaveForfeit
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int Days { get; private set; }
    public DateTime Date { get; private set; }
    public string Reason { get; private set; }

    private LeaveForfeit() { }

    public LeaveForfeit(Guid employeeId, Guid leaveTypeId, int days, string reason, DateTime? date = null)
    {
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        Days = days;
        Reason = reason;
        Date = date ?? DateTime.UtcNow;
    }
}

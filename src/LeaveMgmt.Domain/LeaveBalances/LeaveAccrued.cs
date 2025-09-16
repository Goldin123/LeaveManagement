namespace LeaveMgmt.Domain.LeaveBalances;

public sealed class LeaveAccrued
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int Days { get; private set; }
    public DateTime Date { get; private set; }

    private LeaveAccrued() { }

    public LeaveAccrued(Guid employeeId, Guid leaveTypeId, int days, DateTime? date = null)
    {
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        Days = days;
        Date = date ?? DateTime.UtcNow;
    }
}

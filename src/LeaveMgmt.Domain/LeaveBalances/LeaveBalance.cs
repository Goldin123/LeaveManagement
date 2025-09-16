namespace LeaveMgmt.Domain.LeaveBalances;

public sealed class LeaveBalance
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int DaysAvailable { get; private set; }
    public int DaysTaken { get; private set; }
    public int DaysAccrued { get; private set; }
    public int DaysForfeited { get; private set; }

    private LeaveBalance() { }

    public LeaveBalance(Guid employeeId, Guid leaveTypeId, int openingBalance = 0)
    {
        EmployeeId = employeeId;
        LeaveTypeId = leaveTypeId;
        DaysAvailable = openingBalance;
    }

    public void ApplyAccrual(int days)
    {
        DaysAccrued += days;
        DaysAvailable += days;
    }

    public void ApplyForfeit(int days)
    {
        DaysForfeited += days;
        DaysAvailable = Math.Max(0, DaysAvailable - days);
    }

    public bool Deduct(int days)
    {
        if (DaysAvailable < days) return false;

        DaysTaken += days;
        DaysAvailable -= days;
        return true;
    }
}

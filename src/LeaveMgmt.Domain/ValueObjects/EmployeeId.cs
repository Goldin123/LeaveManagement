namespace LeaveMgmt.Domain.ValueObjects;

public readonly struct EmployeeId(Guid value)
{
    public Guid Value { get; } = value;
    public override string ToString() => Value.ToString();
}

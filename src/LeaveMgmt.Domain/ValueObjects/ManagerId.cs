namespace LeaveMgmt.Domain.ValueObjects;

public readonly struct ManagerId(Guid value)
{
    public Guid Value { get; } = value;
    public override string ToString() => Value.ToString();
}

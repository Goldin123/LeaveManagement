namespace LeaveMgmt.Domain.Holidays;

public sealed class Holiday
{
    public string Name { get; }
    public DateTime Date { get; }

    public Holiday(string name, DateTime date)
    {
        Name = name;
        Date = date;
    }
}

namespace LeaveMgmt.Domain.ValueObjects;

public readonly struct DateRange
{
    public DateOnly From { get; }
    public DateOnly To   { get; }
    public int Days => To.DayNumber - From.DayNumber + 1;

    public DateRange(DateOnly from, DateOnly to)
    {
        if (to < from) throw new ArgumentException("To must be >= From");
        From = from; To = to;
    }

    public bool Overlaps(DateRange other) =>
        From <= other.To && other.From <= To;
}

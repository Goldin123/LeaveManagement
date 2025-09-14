using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Domain.ValueObjects;

public readonly struct DateRange
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    public int Days => End.DayNumber - Start.DayNumber + 1;

    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start) throw new DomainException("End date cannot be before start date.");
        Start = start;
        End = end;
    }

    public bool Overlaps(DateRange other) =>
        Start <= other.End && other.Start <= End;
}

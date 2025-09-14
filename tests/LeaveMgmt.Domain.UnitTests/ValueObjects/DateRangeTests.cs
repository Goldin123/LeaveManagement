using FluentAssertions;
using LeaveMgmt.Domain.ValueObjects;
using LeaveMgmt.Domain.Common;
using Xunit;

namespace LeaveMgmt.Domain.UnitTests.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void ctor_ShouldThrow_WhenEndBeforeStart()
    {
        var act = () => new DateRange(new(2025, 1, 10), new(2025, 1, 9));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Overlaps_ShouldBeTrue_ForIntersectingRanges()
    {
        var a = new DateRange(new(2025, 1, 1), new(2025, 1, 5));
        var b = new DateRange(new(2025, 1, 5), new(2025, 1, 10));
        a.Overlaps(b).Should().BeTrue();
    }
}

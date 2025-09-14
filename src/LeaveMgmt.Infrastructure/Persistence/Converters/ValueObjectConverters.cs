using System.Linq.Expressions;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LeaveMgmt.Infrastructure.Persistence;

// EF-safe expression-based converters so LINQ can translate comparisons
internal static class ValueObjectConverters
{
    // DateRange <-> string  (yyyy-MM-dd|yyyy-MM-dd)
    public static readonly Expression<Func<DateRange, string>> DateRangeToStringExpr =
        dr => dr.Start.ToString("yyyy-MM-dd") + "|" + dr.End.ToString("yyyy-MM-dd");

    public static readonly Expression<Func<string, DateRange>> StringToDateRangeExpr =
        s => new DateRange(
            DateOnly.Parse(s.Substring(0, 10)),
            DateOnly.Parse(s.Substring(11, 10))
        );

    public static readonly ValueConverter<DateRange, string> DateRangeConverter =
        new(DateRangeToStringExpr, StringToDateRangeExpr);

    // EmployeeId <-> Guid
    public static readonly Expression<Func<EmployeeId, Guid>> EmployeeIdToGuidExpr = id => id.Value;
    public static readonly Expression<Func<Guid, EmployeeId>> GuidToEmployeeIdExpr = g => new EmployeeId(g);

    public static readonly ValueConverter<EmployeeId, Guid> EmployeeIdConverter =
        new(EmployeeIdToGuidExpr, GuidToEmployeeIdExpr);

    // ManagerId? <-> Guid?
    public static readonly Expression<Func<ManagerId?, Guid?>> ManagerIdToGuidExpr =
        id => id.HasValue ? id.Value.Value : (Guid?)null;

    public static readonly Expression<Func<Guid?, ManagerId?>> GuidToManagerIdExpr =
        g => g.HasValue ? new ManagerId(g.Value) : (ManagerId?)null;

    public static readonly ValueConverter<ManagerId?, Guid?> ManagerIdNullableConverter =
        new(ManagerIdToGuidExpr, GuidToManagerIdExpr);
}

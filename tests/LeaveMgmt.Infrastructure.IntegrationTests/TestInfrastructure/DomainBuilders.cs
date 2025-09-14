using System.Reflection;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Infrastructure.IntegrationTests.TestInfrastructure;

public static class DomainBuilders
{
    // Try to get a primitive/Guid value out of value objects like EmployeeId.
    private static object Unwrap(object obj)
    {
        var prop = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                      .FirstOrDefault(p => p.Name is "Value" or "Id");
        return prop is null ? obj : (prop.GetValue(obj) ?? obj);
    }

    public static LeaveType BuildLeaveType(
        string name = "Annual",
        string? description = "Annual leave",
        int defaultDays = 15)
    {
        // Try common shapes in order:
        // (string name, string? description, int defaultDays)
        // (string name, int defaultDays)
        // (Guid id, string name, string? description, int defaultDays)
        var ctor =
            typeof(LeaveType).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(c =>
            {
                var p = c.GetParameters();
                return (p.Length == 3 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(string) && p[2].ParameterType == typeof(int))
                    || (p.Length == 2 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(int))
                    || (p.Length == 4 && p[0].ParameterType == typeof(Guid));
            }) ?? throw new InvalidOperationException("No matching LeaveType ctor found.");

        var args = ctor.GetParameters().Length switch
        {
            3 => new object?[] { name, description, defaultDays },
            2 => new object?[] { name, defaultDays },
            4 => new object?[] { Guid.NewGuid(), name, description, defaultDays },
            _ => throw new InvalidOperationException()
        };

        return (LeaveType)ctor.Invoke(args);
    }

    public static LeaveRequest BuildLeaveRequest(
        Guid? leaveTypeId = null,
        EmployeeId? employeeId = null,
        DateRange? range = null,
        string? reason = "Family event",
        LeaveType? leaveTypeAggregate = null)
    {
        var emp = employeeId ?? new EmployeeId(Guid.NewGuid());
        var dr = range ?? new DateRange(DateOnly.FromDateTime(DateTime.UtcNow.Date),
                                          DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(2)));
        var ltId = leaveTypeId ?? Guid.NewGuid();

        // Try shapes:
        // (EmployeeId, Guid leaveTypeId, DateRange, string?)
        // (EmployeeId, LeaveType,       DateRange, string?)
        // (Guid id, EmployeeId, Guid leaveTypeId, DateRange, string?)
        var ctors = typeof(LeaveRequest).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var ctor = ctors.FirstOrDefault(c =>
        {
            var p = c.GetParameters();
            return p.Length == 4 && p[0].ParameterType == typeof(EmployeeId) && p[1].ParameterType == typeof(Guid)
                                 && p[2].ParameterType == typeof(DateRange)
                || p.Length == 4 && p[0].ParameterType == typeof(EmployeeId) && p[1].ParameterType == typeof(LeaveType)
                                 && p[2].ParameterType == typeof(DateRange)
                || p.Length == 5 && p[0].ParameterType == typeof(Guid) && p[1].ParameterType == typeof(EmployeeId)
                                 && p[2].ParameterType == typeof(Guid) && p[3].ParameterType == typeof(DateRange);
        }) ?? throw new InvalidOperationException("No matching LeaveRequest ctor found.");

        var ps = ctor.GetParameters();
        object?[] args = ps.Length switch
        {
            4 when ps[1].ParameterType == typeof(Guid)
                => new object?[] { emp, ltId, dr, reason },
            4 when ps[1].ParameterType == typeof(LeaveType)
                => new object?[] { emp, leaveTypeAggregate ?? BuildLeaveType(), dr, reason },
            5 => new object?[] { Guid.NewGuid(), emp, ltId, dr, reason },
            _ => throw new InvalidOperationException()
        };

        return (LeaveRequest)ctor.Invoke(args);
    }
}

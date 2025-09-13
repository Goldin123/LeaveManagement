namespace LeaveMgmt.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
}

public enum LeaveType { Annual, Sick, Unpaid }

public class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public LeaveType Type { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To   { get; set; }
    public string Status { get; set; } = "Pending";
}

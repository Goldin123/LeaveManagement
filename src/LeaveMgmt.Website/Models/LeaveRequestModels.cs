using System.ComponentModel.DataAnnotations;

namespace LeaveMgmt.Website.Models;

public sealed class SubmitLeaveRequest
{
    public Guid EmployeeId { get; set; }
    [Required] public Guid LeaveTypeId { get; set; }
    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }
    public int Days { get; set; }
    public string Reason { get; set; }
}
public class SubmitLeaveResponse
{
    public Guid Id { get; set; }
}


public sealed class ApproveRequest { [Required] public Guid Id { get; set; } public Guid ManagerId { get; set; } }
public sealed class RejectRequest { [Required] public Guid Id { get; set; } public string Reason { get; set; } }
public sealed class RetractRequest { [Required] public Guid Id { get; set; } }

public sealed class LeaveRequestListItem
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? Days { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected
}

public sealed class LeaveType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class Holiday
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
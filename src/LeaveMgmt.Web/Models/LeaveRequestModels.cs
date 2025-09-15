using System.ComponentModel.DataAnnotations;

namespace LeaveMgmt.Web.Models;

public sealed class SubmitLeaveRequest
{
    [Required] public Guid LeaveTypeId { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}

public sealed class ApproveRequest { [Required] public Guid Id { get; set; } }
public sealed class RejectRequest { [Required] public Guid Id { get; set; } public string? Reason { get; set; } }
public sealed class RetractRequest { [Required] public Guid Id { get; set; } }

public sealed class LeaveRequestListItem
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected
}

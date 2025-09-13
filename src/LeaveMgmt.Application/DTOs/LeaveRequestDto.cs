namespace LeaveMgmt.Application.DTOs;

public sealed class LeaveRequestDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string Type { get; init; } = string.Empty;
    public DateOnly From { get; init; }
    public DateOnly To   { get; init; }
    public string Status { get; init; } = "Pending";
}

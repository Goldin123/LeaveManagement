namespace LeaveMgmt.Api.Endpoints;

public sealed record SubmitLeaveRequestBody(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    int? Days,
    string Reason);

public sealed record ApproveBody(Guid ManagerId);
public sealed record RejectBody(Guid ManagerId, string Reason);
public sealed record RetractBody(Guid EmployeeId);

public sealed record ByEmployeeRoute(Guid EmployeeId);
public sealed record IdRoute(Guid Id);

public sealed record RegisterRequest(string Email, string FullName, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token);
public sealed record LoginBody(string Email, string Password);
public sealed record RegisterBody(string Email, string FullName, string Password);
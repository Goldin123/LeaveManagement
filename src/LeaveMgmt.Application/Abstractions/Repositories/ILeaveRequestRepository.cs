using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Abstractions.Repositories;

public interface ILeaveRequestRepository
{
    Task<Result<LeaveRequest>> CreateAsync(LeaveRequest request, CancellationToken ct = default);
    Task<Result<LeaveRequest?>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<LeaveRequest>>> GetByEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default);
    Task<Result> UpdateAsync(LeaveRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

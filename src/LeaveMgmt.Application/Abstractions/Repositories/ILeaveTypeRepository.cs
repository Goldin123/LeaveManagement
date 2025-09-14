using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;

namespace LeaveMgmt.Application.Abstractions.Repositories;

public interface ILeaveTypeRepository
{
    Task<Result<LeaveType>> CreateAsync(LeaveType type, CancellationToken ct = default);
    Task<Result<LeaveType?>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<LeaveType>>> GetAllAsync(CancellationToken ct = default);
}

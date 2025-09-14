using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.ValueObjects;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveMgmt.Infrastructure.Repositories;

public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly LeaveMgmtDbContext _db;

    public LeaveRequestRepository(LeaveMgmtDbContext db) => _db = db;

    public async Task<Result<LeaveRequest>> CreateAsync(LeaveRequest entity, CancellationToken ct = default)
    {
        try
        {
            await _db.LeaveRequests.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return Result<LeaveRequest>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<LeaveRequest>.Failure($"Failed to create leave request: {ex.Message}");
        }
    }

    public async Task<Result<LeaveRequest?>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var req = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct);
            return Result<LeaveRequest?>.Success(req);
        }
        catch (Exception ex)
        {
            return Result<LeaveRequest?>.Failure($"Failed to get leave request: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<LeaveRequest>>> GetByEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        try
        {
            // Thanks to value converter, EF can translate this equality comparison
            var items = await _db.LeaveRequests
                                .Where(l => l.EmployeeId.Equals(employeeId))
                                 .ToListAsync(ct);

            return Result<IReadOnlyList<LeaveRequest>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<LeaveRequest>>.Failure($"Failed to query leave requests: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(LeaveRequest request, CancellationToken ct = default)
    {
        try
        {
            // Ensure the entity exists (and avoid double tracking)
            var existing = await _db.LeaveRequests
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (existing is null)
                return Result.Failure("Leave request not found.");

            // Attach the incoming aggregate and mark as modified
            _db.Attach(request);
            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure("The leave request was updated by another process. Please reload and try again.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update leave request: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var entity = await _db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
                return Result.Failure("Leave request not found.");

            _db.LeaveRequests.Remove(entity);
            await _db.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure($"Failed to delete leave request due to database constraints: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete leave request: {ex.Message}");
        }
    }
}

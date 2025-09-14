using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LeaveMgmt.Infrastructure.Repositories;

public sealed class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly LeaveMgmtDbContext _db;

    public LeaveTypeRepository(LeaveMgmtDbContext db) => _db = db;

    public async Task<Result<LeaveType>> CreateAsync(LeaveType type, CancellationToken ct = default)
    {
        try
        {
            await _db.LeaveTypes.AddAsync(type, ct);
            await _db.SaveChangesAsync(ct);
            return Result<LeaveType>.Success(type);
        }
        catch (Exception ex)
        {
            return Result<LeaveType>.Failure($"Failed to create leave type: {ex.Message}");
        }
    }

    public async Task<Result<LeaveType?>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var lt = await _db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id, ct);
            return Result<LeaveType?>.Success(lt);
        }
        catch (Exception ex)
        {
            return Result<LeaveType?>.Failure($"Failed to get leave type: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<LeaveType>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await _db.LeaveTypes.OrderBy(t => t.Name).ToListAsync(ct);
            return Result<IReadOnlyList<LeaveType>>.Success(items);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<LeaveType>>.Failure($"Failed to list leave types: {ex.Message}");
        }
    }
}

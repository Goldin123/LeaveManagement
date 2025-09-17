using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Repositories;

/// <summary>
/// Repository for managing LeaveType persistence (CRUD operations).
/// Uses EF Core DbContext and logs all important operations.
/// </summary>
public sealed class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly LeaveMgmtDbContext _db;
    private readonly ILogger<LeaveTypeRepository> _logger;

    public LeaveTypeRepository(LeaveMgmtDbContext db, ILogger<LeaveTypeRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new leave type.
    /// </summary>
    public async Task<Result<LeaveType>> CreateAsync(LeaveType type, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating leave type {LeaveTypeName}", type.Name);

            await _db.LeaveTypes.AddAsync(type, ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Leave type {LeaveTypeName} created successfully with Id {LeaveTypeId}", type.Name, type.Id);

            return Result<LeaveType>.Success(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create leave type {LeaveTypeName}", type.Name);
            return Result<LeaveType>.Failure($"Failed to create leave type: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a leave type by ID.
    /// </summary>
    public async Task<Result<LeaveType?>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching leave type {LeaveTypeId}", id);

            var lt = await _db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id, ct);

            if (lt is null)
                _logger.LogWarning("Leave type {LeaveTypeId} not found", id);
            else
                _logger.LogInformation("Leave type {LeaveTypeId} fetched successfully", id);

            return Result<LeaveType?>.Success(lt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch leave type {LeaveTypeId}", id);
            return Result<LeaveType?>.Failure($"Failed to get leave type: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all leave types ordered by name.
    /// </summary>
    public async Task<Result<IReadOnlyList<LeaveType>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching all leave types");

            var items = await _db.LeaveTypes.OrderBy(t => t.Name).ToListAsync(ct);

            _logger.LogInformation("Successfully fetched {Count} leave types", items.Count);

            return Result<IReadOnlyList<LeaveType>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch leave types");
            return Result<IReadOnlyList<LeaveType>>.Failure($"Failed to list leave types: {ex.Message}");
        }
    }
}

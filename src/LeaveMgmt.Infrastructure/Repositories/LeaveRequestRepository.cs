using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveRequests;
using LeaveMgmt.Domain.ValueObjects;
using LeaveMgmt.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Infrastructure.Repositories;

/// <summary>
/// Repository for managing LeaveRequest persistence (CRUD operations).
/// Uses EF Core DbContext and logs all important operations.
/// </summary>
public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly LeaveMgmtDbContext _db;
    private readonly ILogger<LeaveRequestRepository> _logger;

    public LeaveRequestRepository(LeaveMgmtDbContext db, ILogger<LeaveRequestRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Create a new leave request.
    /// </summary>
    public async Task<Result<LeaveRequest>> CreateAsync(LeaveRequest entity, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating leave request {LeaveRequestId} for employee {EmployeeId}", entity.Id, entity.EmployeeId);

            await _db.LeaveRequests.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Leave request {LeaveRequestId} created successfully", entity.Id);

            return Result<LeaveRequest>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create leave request {LeaveRequestId}", entity.Id);
            return Result<LeaveRequest>.Failure($"Failed to create leave request: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a leave request by ID.
    /// </summary>
    public async Task<Result<LeaveRequest?>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching leave request {LeaveRequestId}", id);

            var req = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct);

            if (req is null)
                _logger.LogWarning("Leave request {LeaveRequestId} not found", id);
            else
                _logger.LogInformation("Leave request {LeaveRequestId} fetched successfully", id);

            return Result<LeaveRequest?>.Success(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch leave request {LeaveRequestId}", id);
            return Result<LeaveRequest?>.Failure($"Failed to get leave request: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all leave requests by employee ID.
    /// </summary>
    public async Task<Result<IReadOnlyList<LeaveRequest>>> GetByEmployeeAsync(EmployeeId employeeId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching leave requests for employee {EmployeeId}", employeeId.Value);

            // EF translates value object comparison thanks to configured converter
            var items = await _db.LeaveRequests
                                .Where(l => l.EmployeeId.Equals(employeeId))
                                .ToListAsync(ct);

            _logger.LogInformation("Fetched {Count} leave requests for employee {EmployeeId}", items.Count, employeeId.Value);

            return Result<IReadOnlyList<LeaveRequest>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch leave requests for employee {EmployeeId}", employeeId.Value);
            return Result<IReadOnlyList<LeaveRequest>>.Failure($"Failed to query leave requests: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing leave request.
    /// </summary>
    public async Task<Result> UpdateAsync(LeaveRequest request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating leave request {LeaveRequestId}", request.Id);

            // Ensure the entity exists (and avoid double tracking)
            var existing = await _db.LeaveRequests
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (existing is null)
            {
                _logger.LogWarning("Leave request {LeaveRequestId} not found for update", request.Id);
                return Result.Failure("Leave request not found.");
            }

            _db.Attach(request);
            _db.Entry(request).State = EntityState.Modified;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Leave request {LeaveRequestId} updated successfully", request.Id);

            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict updating leave request {LeaveRequestId}", request.Id);
            return Result.Failure("The leave request was updated by another process. Please reload and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update leave request {LeaveRequestId}", request.Id);
            return Result.Failure($"Failed to update leave request: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a leave request by ID.
    /// </summary>
    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting leave request {LeaveRequestId}", id);

            var entity = await _db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                _logger.LogWarning("Leave request {LeaveRequestId} not found for deletion", id);
                return Result.Failure("Leave request not found.");
            }

            _db.LeaveRequests.Remove(entity);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Leave request {LeaveRequestId} deleted successfully", id);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database constraint prevented deletion of leave request {LeaveRequestId}", id);
            return Result.Failure($"Failed to delete leave request due to database constraints: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete leave request {LeaveRequestId}", id);
            return Result.Failure($"Failed to delete leave request: {ex.Message}");
        }
    }
}

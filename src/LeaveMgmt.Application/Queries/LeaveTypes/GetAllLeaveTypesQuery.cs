using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Queries.LeaveTypes;

/// <summary>
/// Query request: get all available leave types.
/// </summary>
public sealed record GetAllLeaveTypesQuery : IRequest<Result<IReadOnlyList<LeaveType>>>;

/// <summary>
/// Handles retrieval of leave types. Calls the repository,
/// validates result, and logs each important step.
/// </summary>
public sealed class GetAllLeaveTypesHandler(ILeaveTypeRepository repo, ILogger<GetAllLeaveTypesHandler> logger)
    : IRequestHandler<GetAllLeaveTypesQuery, Result<IReadOnlyList<LeaveType>>>
{
    public async Task<Result<IReadOnlyList<LeaveType>>> Handle(GetAllLeaveTypesQuery query, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation("Fetching all leave types");

            // 1) Fetch from repository
            var res = await repo.GetAllAsync(ct);
            if (!res.IsSuccess || res.Value is null)
            {
                logger.LogWarning("Failed to fetch leave types. Error: {Error}", res.Error);
                return Result<IReadOnlyList<LeaveType>>.Failure(res.Error ?? "Failed to load leave types.");
            }

            // 2) Return result
            logger.LogInformation("Successfully fetched {Count} leave types", res.Value.Count);
            return Result<IReadOnlyList<LeaveType>>.Success(res.Value);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Unhandled exception while fetching leave types");
            return Result<IReadOnlyList<LeaveType>>.Failure($"Query failed: {ex.Message}");
        }
    }
}

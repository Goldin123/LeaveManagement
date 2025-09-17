using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Queries.LeaveRequests.GetByEmployee;

/// <summary>
/// Query request: get all leave requests for a specific employee.
/// </summary>
public sealed record GetLeaveRequestsByEmployeeQuery(Guid EmployeeId)
    : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;

/// <summary>
/// Handles retrieval of leave requests for an employee. Calls the repository,
/// maps domain entities to DTOs, and logs each important step.
/// </summary>
public sealed class GetLeaveRequestsByEmployeeHandler(ILeaveRequestRepository repo, ILogger<GetLeaveRequestsByEmployeeHandler> logger)
    : IRequestHandler<GetLeaveRequestsByEmployeeQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeaveRequestsByEmployeeQuery q, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation("Fetching leave requests for employee {EmployeeId}", q.EmployeeId);

            // 1) Fetch from repository
            var res = await repo.GetByEmployeeAsync(new EmployeeId(q.EmployeeId), ct);
            if (!res.IsSuccess)
            {
                logger.LogWarning("Failed to fetch leave requests for {EmployeeId}. Error: {Error}", q.EmployeeId, res.Error);
                return Result<IReadOnlyList<LeaveRequestDto>>.Failure(res.Error!);
            }

            // 2) Map to DTOs
            var list = res.Value!.Select(LeaveRequestDto.FromDomain).ToList();

            logger.LogInformation("Successfully fetched {Count} leave requests for employee {EmployeeId}", list.Count, q.EmployeeId);

            return Result<IReadOnlyList<LeaveRequestDto>>.Success(list);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Unhandled exception while fetching leave requests for employee {EmployeeId}", q.EmployeeId);
            return Result<IReadOnlyList<LeaveRequestDto>>.Failure($"Query failed: {ex.Message}");
        }
    }
}

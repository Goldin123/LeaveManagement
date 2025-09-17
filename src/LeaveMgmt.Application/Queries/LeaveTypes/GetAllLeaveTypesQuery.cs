using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.LeaveTypes;

namespace LeaveMgmt.Application.Queries.LeaveTypes;

public sealed record GetAllLeaveTypesQuery : IRequest<Result<IReadOnlyList<LeaveType>>>;

public sealed class GetAllLeaveTypesHandler(ILeaveTypeRepository repo)
    : IRequestHandler<GetAllLeaveTypesQuery, Result<IReadOnlyList<LeaveType>>>
{
    public async Task<Result<IReadOnlyList<LeaveType>>> Handle(GetAllLeaveTypesQuery query, CancellationToken ct)
    {
        try
        {
            var res = await repo.GetAllAsync(ct);
            if (!res.IsSuccess || res.Value is null)
                return Result<IReadOnlyList<LeaveType>>.Failure(res.Error ?? "Failed to load leave types.");

            return Result<IReadOnlyList<LeaveType>>.Success(res.Value);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<LeaveType>>.Failure($"Query failed: {ex.Message}");
        }
    }
}

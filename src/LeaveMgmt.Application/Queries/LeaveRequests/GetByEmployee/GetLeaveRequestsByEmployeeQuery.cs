using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Application.DTOs;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.ValueObjects;

namespace LeaveMgmt.Application.Queries.LeaveRequests.GetByEmployee;

public sealed record GetLeaveRequestsByEmployeeQuery(Guid EmployeeId)
    : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;

public sealed class GetLeaveRequestsByEmployeeHandler(ILeaveRequestRepository repo)
    : IRequestHandler<GetLeaveRequestsByEmployeeQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeaveRequestsByEmployeeQuery q, CancellationToken ct)
    {
        try
        {
            var res = await repo.GetByEmployeeAsync(new EmployeeId(q.EmployeeId), ct);
            if (!res.IsSuccess) return Result<IReadOnlyList<LeaveRequestDto>>.Failure(res.Error!);

            var list = res.Value!.Select(LeaveRequestDto.FromDomain).ToList();
            return Result<IReadOnlyList<LeaveRequestDto>>.Success(list);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<LeaveRequestDto>>.Failure($"Query failed: {ex.Message}");
        }
    }
}

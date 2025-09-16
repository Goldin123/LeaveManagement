// File: LeaveMgmt.Application/Queries/LeaveTypes/GetAllLeaveTypesQuery.cs
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
        return await repo.GetAllAsync(ct);
    }
}

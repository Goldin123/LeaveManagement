using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Entities;
using LeaveMgmt.Domain.LeaveRequests;

namespace LeaveMgmt.Application.Features.LeaveRequests;
/*
public sealed record CreateLeaveRequest(
    Guid EmployeeId,
    string Type,
    DateOnly From,
    DateOnly To
) : IRequest<Result<Guid>>;

public sealed class CreateLeaveRequestHandler : IRequestHandler<CreateLeaveRequest, Result<Guid>>
{
    private readonly IRepository<LeaveRequest> _repo;

    public CreateLeaveRequestHandler(IRepository<LeaveRequest> repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<LeaveType>(request.Type, ignoreCase: true, out var parsed))
            return Result<Guid>.Failure($"Unknown leave type '{request.Type}'.");

        var entity = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = parsed,
            From = request.From,
            To   = request.To,
            Status = "Pending"
        };

        await _repo.AddAsync(entity, ct);
        return Result<Guid>.Success(entity.Id);
    }
}
*/
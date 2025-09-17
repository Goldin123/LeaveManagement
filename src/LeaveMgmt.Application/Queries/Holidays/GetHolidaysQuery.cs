using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Holidays;

namespace LeaveMgmt.Application.Queries.Holidays;

public sealed record GetHolidaysQuery(int Year) : IRequest<Result<IReadOnlyList<Holiday>>>;

public sealed class GetHolidaysHandler(IHolidayRepository repo)
    : IRequestHandler<GetHolidaysQuery, Result<IReadOnlyList<Holiday>>>
{
    public async Task<Result<IReadOnlyList<Holiday>>> Handle(GetHolidaysQuery q, CancellationToken ct)
    {
        try
        {
            var list = await repo.GetHolidaysAsync(q.Year, ct);
            return Result<IReadOnlyList<Holiday>>.Success(list);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Holiday>>.Failure($"Failed to get holidays: {ex.Message}");
        }
    }
}

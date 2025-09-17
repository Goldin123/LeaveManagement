using LeaveMgmt.Domain.Holidays;

namespace LeaveMgmt.Application.Abstractions.Repositories;

public interface IHolidayRepository
{
    Task<IReadOnlyList<Holiday>> GetHolidaysAsync(int year, CancellationToken ct = default);
}

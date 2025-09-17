using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Application.Abstractions.Repositories;
using LeaveMgmt.Domain;
using LeaveMgmt.Domain.Holidays;
using Microsoft.Extensions.Logging;

namespace LeaveMgmt.Application.Queries.Holidays;

/// <summary>
/// Query request: get public holidays for a specific year.
/// </summary>
public sealed record GetHolidaysQuery(int Year) : IRequest<Result<IReadOnlyList<Holiday>>>;

/// <summary>
/// Handles retrieval of holidays. Calls the repository, returns results,
/// and logs each important step.
/// </summary>
public sealed class GetHolidaysHandler(IHolidayRepository repo, ILogger<GetHolidaysHandler> logger)
    : IRequestHandler<GetHolidaysQuery, Result<IReadOnlyList<Holiday>>>
{
    public async Task<Result<IReadOnlyList<Holiday>>> Handle(GetHolidaysQuery q, CancellationToken ct)
    {
        try
        {
            // Log attempt
            logger.LogInformation("Fetching holidays for year {Year}", q.Year);

            // 1) Call repository to load holidays
            var list = await repo.GetHolidaysAsync(q.Year, ct);

            logger.LogInformation("Successfully fetched {Count} holidays for year {Year}", list.Count, q.Year);

            return Result<IReadOnlyList<Holiday>>.Success(list);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected runtime exceptions
            logger.LogError(ex, "Failed to fetch holidays for year {Year}", q.Year);
            return Result<IReadOnlyList<Holiday>>.Failure($"Failed to get holidays: {ex.Message}");
        }
    }
}

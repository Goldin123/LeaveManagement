using Microsoft.Extensions.DependencyInjection;
using LeaveMgmt.Application.Abstractions;
using LeaveMgmt.Domain.Entities;
using LeaveMgmt.Infrastructure.Services;

namespace LeaveMgmt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, SystemClock>();

        // repositories
        services.AddSingleton<IRepository<LeaveRequest>, InMemoryRepository<LeaveRequest>>();

        return services;
    }
}

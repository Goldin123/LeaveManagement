using LeaveMgmt.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveMgmt.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSiteServices(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<LeaveRequestService>();
        services.AddScoped<LeaveTypeService>();
        services.AddScoped<UserService>();
        services.AddScoped<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage>();
        return services;
    }
}

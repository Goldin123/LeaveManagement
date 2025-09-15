using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace LeaveMgmt.Web.Extensions;

public static class MudBlazorExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        // Example: HttpClients for your API
        services.AddHttpClient("LeaveMgmtApi", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7186/");
        });

        return services;
    }
}

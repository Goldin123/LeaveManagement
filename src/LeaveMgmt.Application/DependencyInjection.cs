using System.Reflection;
using LeaveMgmt.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveMgmt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        // Auto-register all IRequestHandler<,> found in this assembly
        var asm = Assembly.GetExecutingAssembly();
        var handlerInterface = typeof(IRequestHandler<,>);
        foreach (var t in asm.GetTypes())
        {
            var hi = t.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            if (hi is not null && t is { IsAbstract: false, IsInterface: false })
            {
                services.AddTransient(hi, t);
            }
        }

        return services;
    }
}

// File: LeaveMgmt.Application/DependencyInjection.cs
using System.Reflection;
using FluentValidation;
using LeaveMgmt.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveMgmt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // custom mediator
        services.AddSingleton<IMediator, Mediator>();

        var asm = Assembly.GetExecutingAssembly();

        // register all IRequestHandler<,>
        var handlerTypes = asm.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Service = i, Impl = t }));

        foreach (var h in handlerTypes)
        {
            services.AddScoped(h.Service, h.Impl);
        }

        // register FluentValidation validators
        var validatorTypes = asm.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new { Service = i, Impl = t }));

        foreach (var v in validatorTypes)
        {
            services.AddScoped(v.Service, v.Impl);
        }

        return services;
    }
}

using System.Reflection;
using FluentValidation;
using LeaveMgmt.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveMgmt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();

        var asm = Assembly.GetExecutingAssembly();

        // Handlers
        var handlerTypes = asm.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Service = i, Impl = t }));

        foreach (var h in handlerTypes) services.AddScoped(h.Service, h.Impl);

        // Validators
        var validatorTypes = asm.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new { Service = i, Impl = t }));

        foreach (var v in validatorTypes) services.AddScoped(v.Service, v.Impl);

        // Pipeline behaviors (order: auth, then validation, then handler)
        //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

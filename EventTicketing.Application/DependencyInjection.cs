using EventTicketing.Application.Abstractions.Behaviors;
using EventTicketing.Application.Abstractions.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISender, Mediator>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        var assembly = typeof(DependencyInjection).Assembly;
        var openHandler = typeof(IRequestHandler<,>);
        var openValidator = typeof(IValidator<>);
        var openDomainEventHandler = typeof(IDomainEventHandler<>);

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                var def = iface.GetGenericTypeDefinition();
                if (def == openHandler || def == openValidator || def == openDomainEventHandler)
                    services.AddScoped(iface, type);
            }
        }

        return services;
    }
}

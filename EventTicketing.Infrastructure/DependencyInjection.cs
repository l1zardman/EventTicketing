using EventTicketing.Application.Abstractions;
using EventTicketing.Application.Events;
using EventTicketing.Infrastructure.Events;
using EventTicketing.Infrastructure.Payments;
using EventTicketing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<DomainEventDispatchInterceptor>();

        services.AddDbContext<EventsDbContext>((sp, options) =>
            options.UseSqlite(connectionString)
                   .AddInterceptors(sp.GetRequiredService<DomainEventDispatchInterceptor>()));

        services.AddScoped<IEventReadRepository, EventReadRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentGateway, FakePaymentGatewayAdapter>();

        return services;
    }
}

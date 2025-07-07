using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

public static class MessagingServiceCollectionExtensions
{
    // One event aggregator per scope (e.g. per web request,
    // per game scene, or per job handler)
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddScoped<Events>();
        services.AddScoped<IMediator, Mediator>();

        // open-generic registrations for handlers
        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
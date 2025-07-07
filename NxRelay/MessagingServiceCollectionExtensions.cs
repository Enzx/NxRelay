using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

/// <summary>
/// Extension methods for registering messaging services with DI.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    // One event aggregator per scope (e.g. per web request,
    // per game scene, or per job handler)
    /// <summary>
    /// Registers the Events aggregator, Mediator and any discovered handlers in the DI container.
    /// </summary>
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

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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
    public static IServiceCollection AddMessaging(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<Events>();
        services.AddScoped<IMediator, Mediator>();

        if (assemblies is { Length: > 0 })
        {
            // open-generic registrations for handlers from provided assemblies
            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
        else
        {
            // fallback to scanning application dependencies
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
        }

        return services;
    }}
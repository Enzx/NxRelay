using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Events aggregator, Mediator and any discovered handlers in the DI container.
    /// If you pass one or more <paramref name="extraAssemblies"/>, their
    /// IRequestHandler&lt;,&gt; and IHandler&lt;&gt; implementations are picked up as well.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        params Assembly[] extraAssemblies)
    {
        services.AddScoped<Events>();
        services.AddScoped<IMediator, Mediator>();

        //Assemblies we always scan: everything that’s part of the current app
        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo(typeof(IRequestHandler)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        //Assemblies supplied explicitly (plugins, test assemblies, etc.)
        if (extraAssemblies.Length <= 0 ) return services;
        {
            services.Scan(scan => scan
                .FromAssemblies(extraAssemblies)
                .AddClasses(c => c.AssignableTo(typeof(IHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(extraAssemblies)
                .AddClasses(c => c.AssignableTo(typeof(IRequestHandler)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        return services;
    }
}
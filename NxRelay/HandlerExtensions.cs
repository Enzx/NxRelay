using System.Linq;

namespace NxRelay;

/// <summary>
/// Convenience extension methods for subscribing with callbacks.
/// </summary>
public static class HandlerExtensions
{
    /// <summary>Subscribes a simple <see cref="Action{T}"/> as a handler.</summary>
    public static IAsyncDisposable Subscribe<TMessage>(this ISubscriber<TMessage> subscriber, Action<TMessage> action)
        where TMessage : notnull
    {
        return subscriber.Subscribe(new Handler<TMessage>(action, null));
    }

    /// <summary>Subscribes a handler with an additional filter.</summary>
    public static IAsyncDisposable Subscribe<TMessage>(this ISubscriber<TMessage> subscriber, Action<TMessage> action,
        Filter<TMessage> filter) where TMessage : notnull
    {
        return subscriber.Subscribe(new Handler<TMessage>(action, filter));
    }
}
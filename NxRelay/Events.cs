using System.Collections.Concurrent;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace NxRelay;

/// <summary>
/// Provides a loosely coupled event aggregation system.
/// </summary>
public interface ISubscriber
{
    IAsyncDisposable Subscribe<TMessage>(in IHandler<TMessage> handler) where TMessage : notnull;
    IAsyncDisposable Subscribe<TMessage>(Action<TMessage> action, Filter<TMessage> filter) where TMessage : notnull;
    IAsyncDisposable Subscribe<TMessage>(Action<TMessage> action, params Filter<TMessage>[] filters) where TMessage : notnull;
}

/// <summary>
/// Default implementation combining publisher and subscriber behaviour.
/// </summary>
public sealed class Events : IDisposable, ISubscriber, IPublisher
{
    private readonly ConcurrentDictionary<Type, IPublisher?> _brokers = new(4, 10);

    /// <summary>
    /// Publishes a message to all subscribers of the specified type.
    /// </summary>
    public async Task<bool> Publish<TMessage>(TMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message), "Message cannot be null");

        Type messageType = typeof(TMessage);

        if (!_brokers.TryGetValue(messageType, out IPublisher? publisher)) return false;
        switch (publisher)
        {
            case IPublisher<TMessage> templatePublisher:
                await templatePublisher.Publish(message).ConfigureAwait(false);
                return true;
            case null:
                throw new InvalidOperationException($"No publisher found for message type {message.GetType()}");
            default:
                await publisher.Publish(message).ConfigureAwait(false);
                return true;
        }
    }

    public IAsyncDisposable Subscribe<TMessage>(in IHandler<TMessage> handler) where TMessage : notnull
    {
        IPublisher? broker = _brokers.GetOrAdd(typeof(TMessage), _ => new Broker<TMessage>());
        if (broker is Broker<TMessage> templateBroker) return templateBroker.Subscribe(handler);

        throw new InvalidOperationException(
            "Cannot subscribe to a message type that is not of the same type as the broker");
    }
    

    public IAsyncDisposable Subscribe<T>(Action<T> action, Filter<T> filter) where T : notnull
    {
        return Subscribe(new Handler<T>(action, filter));
    }

    public IAsyncDisposable Subscribe<TMessage>(Action<TMessage> action, params Filter<TMessage>[] filters) where TMessage : notnull
    {
        return Subscribe(new Handler<TMessage>(action, new CompositeFilter<TMessage, Filter<TMessage>>(filters)));
    }


    public void Dispose()
    {
        foreach (KeyValuePair<Type, IPublisher?> broker in _brokers)
        {
            IDisposable? disposable = broker.Value as IDisposable;
            disposable?.Dispose();
        }
    }

    public async ValueTask Publish(object message, CancellationToken ct = default)
    {
        Type messageType = message.GetType();
        if (_brokers.TryGetValue(messageType, out IPublisher? publisher))
        {
            if (publisher is IPublisher<object> templatePublisher)
            {
                await templatePublisher.Publish(message, ct).ConfigureAwait(false);
            }
            else
            {
                // If the publisher is not of type IPublisher<TMessage>, we assume it can handle the message
                if (publisher is null)
                    throw new InvalidOperationException($"No publisher found for message type {messageType}");
                await publisher.Publish(message, ct).ConfigureAwait(false);
            }
        }
    }
}
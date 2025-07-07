using System.Buffers;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

/// <summary>
///  A message broker that allows for the publishing and subscribing of messages
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public sealed class Broker<TMessage>(IServiceProvider? sp = null)
    : IPublisher<TMessage>, ISubscriber<TMessage>, IDisposable
{
    private readonly object _gate = new();

    private readonly ConcurrentDictionary<long, IHandler<TMessage>> _handlers = new(Environment.ProcessorCount, 128);

    private long _nextId;

    public IDisposable Subscribe<T>() where T : IHandler<TMessage>
    {
        if (sp is null)
            throw new InvalidOperationException("ServiceProvider is not available. Cannot resolve handler.");
        IHandler<TMessage> handler = sp.GetRequiredService<T>();
        return Subscribe(handler);
    }

    /// <summary>
    /// Publishes a message to all subscribers
    /// </summary>
    /// <param name="message">Template message type</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    public async ValueTask Publish(TMessage message, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (_handlers.IsEmpty) return;
        }

        int count = 0;
        ArrayPool<Task> pool = ArrayPool<Task>.Shared;
        Task[]? rented = null;

        try
        {
            lock (_gate)
            {
                rented = pool.Rent(_handlers.Count);
                foreach (IHandler<TMessage> h in _handlers.Values)
                {
                    if (!h.Filter(message)) continue;
                    ValueTask vt = h.HandleAsync(message, ct);
                    if (!vt.IsCompletedSuccessfully)
                        rented[count++] = vt.AsTask();
                }
            }


            if (count > 0)
                await Task.WhenAll(rented).ConfigureAwait(false);
        }
        finally
        {
            if (rented is not null) pool.Return(rented, true);
        }
    }

    public ValueTask Publish(object message, CancellationToken ct = default)
    {
        if (message is not TMessage typedMessage)
            throw new ArgumentException($"Message must be of type {typeof(TMessage)}", nameof(message));

        return Publish(typedMessage, ct);
    }

    /// <summary>
    /// Subscribes to the message type
    /// </summary>
    ///<param name="handler">a delegate to a function</param>
    public IDisposable Subscribe(IHandler<TMessage> handler)
    {
        long id = Interlocked.Increment(ref _nextId);
        lock (_gate)
        {
            if (_handlers.TryGetValue(id, out _))
                throw new InvalidOperationException($"Handler with ID {id} already exists.");

            _handlers[id] = handler;
        }

        return new SubscriptionToken<TMessage>(id, this);
    }

    public void Unsubscribe(SubscriptionToken<TMessage> token)
    {
        ArgumentNullException.ThrowIfNull(token);
        token.Dispose();
    }

    /// <summary>
    /// Removes the handler with the specified id if it exists.
    /// Missing handlers are ignored so disposing a token twice is safe.
    /// </summary>
    public void Unsubscribe(long id)
    {
        lock (_gate)
        {
            if (_handlers.TryRemove(id, out IHandler<TMessage>? handler))
                if (handler is IDisposable disposable)
                    disposable.Dispose();
            // ignore missing handlers to allow double disposal
            // of subscription tokens without throwing
        }
    }


    /// <summary>
    /// Disposes of all handlers
    /// </summary>
    public void Dispose()
    {
        lock (_gate)
        {
            foreach (KeyValuePair<long, IHandler<TMessage>> handler in _handlers)
                if (handler.Value is IDisposable disposable)
                    disposable.Dispose();

            _handlers.Clear();
            _nextId = 0;
            if (sp is IDisposable disposableSp) disposableSp.Dispose();
        }
    }
}
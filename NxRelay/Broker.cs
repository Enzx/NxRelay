using System.Buffers;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

/// <summary>
///  A message broker that allows for the publishing and subscribing of messages
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public sealed class Broker<TMessage>(IServiceProvider? sp = null)
    : IPublisher<TMessage>, ISubscriber<TMessage>, IAsyncDisposable where TMessage : notnull
{
    private readonly object _mutex = new();

    private readonly ConcurrentDictionary<long, IHandler<TMessage>> _handlers = new(Environment.ProcessorCount, 128);

    private long _nextHandlerId;

    public IAsyncDisposable Subscribe<T>() where T : IHandler<TMessage>
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
        lock (_mutex)
        {
            if (_handlers.IsEmpty) return;
        }

        int count = 0;
        ArrayPool<Task> pool = ArrayPool<Task>.Shared;
        Task[]? rented = null;

        try
        {
            lock (_mutex)
            {
                rented = pool.Rent(_handlers.Count);
                foreach (IHandler<TMessage> h in _handlers.Values)
                {
                    if (!h.Filter(message)) continue;
                    ValueTask vt = h.HandleAsync(message, ct);
                    if (!vt.IsCompletedSuccessfully)
                    {
                        rented[count++] = vt.AsTask();
                    }
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
        {
            throw new ArgumentException($"Message must be of type {typeof(TMessage)}", nameof(message));
        }

        return Publish(typedMessage, ct);
    }

    /// <summary>
    /// Subscribes to the message type
    /// </summary>
    ///<param name="handler">a delegate to a function</param>
    public IAsyncDisposable Subscribe(IHandler<TMessage> handler)
    {
        long id = Interlocked.Increment(ref _nextHandlerId);
        lock (_mutex)
        {
            if (_handlers.TryGetValue(id, out _))
            {
                throw new InvalidOperationException($"Handler with ID {id} already exists.");
            }

            _handlers[id] = handler;
        }

        return new SubscriptionToken<TMessage>(id, this);
    }

    /// <summary>
    /// Removes the handler with the specified id if it exists.
    /// Missing handlers are ignored so disposing a token twice is safe.
    /// </summary>
    public async ValueTask Unsubscribe(long id)
    {
        IHandler<TMessage>? handler;
        lock (_mutex)
        {
            if (!_handlers.TryRemove(id, out handler)) return;
        }

        if (handler is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (handler is IDisposable syncDisposable)
        {
            syncDisposable.Dispose();
        }
        // else
        // {
        //     // If the handler is neither async nor sync disposable, we just ignore it.
        //     // This is a no-op, but we could log a warning if needed.
        // }
    }


    /// <summary>
    /// Disposes of all handlers
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        IEnumerable<Task> tasks;
        lock (_mutex)
        {
            tasks = _handlers.Values.OfType<IAsyncDisposable>()
                .Select(h => h.DisposeAsync().AsTask());
        }

        await Task.WhenAll(tasks);
    }

    public override string ToString()
    {
        lock (_mutex)
        {
            return $"Broker<{typeof(TMessage).Name}> with {_handlers.Count} handlers";
        }
    }
}
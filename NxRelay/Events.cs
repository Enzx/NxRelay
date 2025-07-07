using System.Collections.Concurrent;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace NxRelay
{
    public interface ISubscriber
    {
        IDisposable Subscribe<TMessage>(in IHandler<TMessage> handler);
        IDisposable Subscribe<TMessage>(Action<TMessage> action);
        IDisposable Subscribe<TMessage>(Action<TMessage> action, Filter<TMessage> filter);
        IDisposable Subscribe<TMessage>(Action<TMessage> action, params Filter<TMessage>[] filters);
    }

    public sealed class Events : IDisposable, ISubscriber, IPublisher
    {
        private readonly ConcurrentDictionary<Type, IPublisher?> _brokers = new(4, 10);

        public async Task<bool> Publish<TMessage>(TMessage message)
        {

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null");
            }

            Type messageType = typeof(TMessage);
            
            if (!_brokers.TryGetValue(messageType, out IPublisher? publisher)) return false;
            switch (publisher)
            {
                case IPublisher<TMessage> templatePublisher:
                    await templatePublisher.Publish(message);
                    return  true;
                // If the publisher is not of type IPublisher<TMessage>, we assume it can handle the message
                case null:
                    throw new InvalidOperationException($"No publisher found for message type {message.GetType()}");
                default:
                    await publisher.Publish(message);
                    return  true;
            }
        }

        public IDisposable Subscribe<TMessage>(in IHandler<TMessage> handler)
        {
            IPublisher? broker = _brokers.GetOrAdd(typeof(TMessage), _ => new Broker<TMessage>());
            if (broker is Broker<TMessage> templateBroker)
            {
                return templateBroker.Subscribe(handler);
            }

            throw new InvalidOperationException(
                "Cannot subscribe to a message type that is not of the same type as the broker");
        }

        public IDisposable Subscribe<TMessage>(Action<TMessage> action)
        {
            return Subscribe(new Handler<TMessage>(action));
        }

        public IDisposable Subscribe<T>(Action<T> action, Filter<T> filter)
        {
            return Subscribe(new Handler<T>(action, filter));
        }

        public IDisposable Subscribe<TMessage>(Action<TMessage> action, params Filter<TMessage>[] filters)
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
                    await templatePublisher.Publish(message, ct);
                }
                else
                {
                    // If the publisher is not of type IPublisher<TMessage>, we assume it can handle the message
                    if (publisher is null)
                        throw new InvalidOperationException($"No publisher found for message type {messageType}");
                    await publisher.Publish(message, ct);
                }
            }
        }

      
    }
}
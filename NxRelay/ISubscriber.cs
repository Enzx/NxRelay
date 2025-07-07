namespace NxRelay;

/// <summary>
/// Typed subscription manager used by the broker.
/// </summary>
public interface ISubscriber<TMessage>
{
    /// <summary>Registers a handler for the specified message type.</summary>
    IDisposable Subscribe(IHandler<TMessage> message);

    /// <summary>Cancels a previous subscription.</summary>
    void Unsubscribe(SubscriptionToken<TMessage> token);
}
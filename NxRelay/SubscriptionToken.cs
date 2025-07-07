namespace NxRelay;

/// <summary>
/// A token representing a subscription.
/// </summary>
public readonly struct SubscriptionToken<TMessage> : IDisposable
{
    public static readonly SubscriptionToken<TMessage> Empty = new(-1, null);
    private readonly Broker<TMessage>? _broker;
    private long Id { get; }


    internal SubscriptionToken(long tokenId, Broker<TMessage>? broker)
    {
        Id = tokenId;
        _broker = broker;
    }

    public void Dispose()
    {
        _broker?.Unsubscribe(Id);
    }
}
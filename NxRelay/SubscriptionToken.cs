namespace NxRelay;

/// <summary>
/// A token representing a subscription.
/// </summary>
public class SubscriptionToken<TMessage> : IAsyncDisposable where TMessage : notnull
{
    public static readonly SubscriptionToken<TMessage> Empty = new(-1, null);
    private readonly Broker<TMessage>? _broker;
    private long Id { get; }


    internal SubscriptionToken(long tokenId, Broker<TMessage>? broker)
    {
        Id = tokenId;
        _broker = broker;
    }

 

    public async ValueTask DisposeAsync()
    {
        if (_broker is null || Id < 0)
            return;
        
        await _broker.Unsubscribe(Id);
        
    }
}
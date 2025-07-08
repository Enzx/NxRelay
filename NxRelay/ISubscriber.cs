namespace NxRelay;

/// <summary>
/// Typed subscription manager used by the broker.
/// </summary>
public interface ISubscriber<out TMessage> where TMessage : notnull
{
    /// <summary>Registers a handler for the specified message type.</summary>
    IAsyncDisposable Subscribe(IHandler<TMessage> message);

}
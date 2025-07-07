namespace NxRelay;

/// <summary>
/// Non-generic publisher interface used for dispatching messages.
/// </summary>
public interface IPublisher
{
    /// <summary>Publishes an untyped message.</summary>
    ValueTask Publish(object message, CancellationToken ct = default);
}

/// <summary>
/// Generic publisher for strongly typed messages.
/// </summary>
public interface IPublisher<in TMessage> : IPublisher
{
    /// <summary>Publishes a typed message.</summary>
    ValueTask Publish(TMessage message, CancellationToken ct = default);
}
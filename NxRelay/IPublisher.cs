namespace NxRelay
{
    public interface IPublisher
    {
        ValueTask Publish(object message, CancellationToken ct = default);
    }
    public interface IPublisher<in TMessage> : IPublisher
    {
        ValueTask Publish(TMessage message, CancellationToken ct = default);
    }
}

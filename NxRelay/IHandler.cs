namespace NxRelay;

public interface IHandler<in TMessage> : IAsyncDisposable
    where TMessage : notnull
{
    ValueTask HandleAsync(TMessage message, CancellationToken ct);
    bool Filter(TMessage message);
}
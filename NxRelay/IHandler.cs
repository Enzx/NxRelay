namespace NxRelay;

public interface IHandler<in TMessage> : IAsyncDisposable
    where TMessage : notnull
{
    ValueTask Handle(TMessage message, CancellationToken ct);
    bool Filter(TMessage message);
}
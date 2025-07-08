namespace NxRelay;

public sealed class Handler<TMessage>(Action<TMessage> callback, Filter<TMessage>? filter = null)
    : IHandler<TMessage>, IDisposable where TMessage : notnull
{
    private Action<TMessage>? _callback = callback ?? throw new ArgumentNullException(nameof(callback));

    public bool Filter(TMessage msg)
    {
        return filter?.Apply(msg) ?? true;
    }

    public ValueTask HandleAsync(TMessage msg, CancellationToken _)
    {
        _callback?.Invoke(msg);
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _callback, null);
    }

    public async ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _callback, null);

        await ValueTask.CompletedTask.ConfigureAwait(false);
    }
}
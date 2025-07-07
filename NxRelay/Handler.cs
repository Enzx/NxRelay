namespace NxRelay
{
    public sealed class Handler<TMessage>(Action<TMessage> callback, Filter<TMessage>? filter = null)
        : IHandler<TMessage>, IDisposable
    {
        private Action<TMessage>? _callback = callback ?? throw new ArgumentNullException(nameof(callback));

        public bool Filter(TMessage msg) => filter?.Apply(msg) ?? true;

        public ValueTask HandleAsync(TMessage msg, CancellationToken _)
        {
            Action<TMessage>? callback = Interlocked.Exchange(ref _callback, null);
            callback?.Invoke(msg);
            return default;
        }

        public void Dispose() => Interlocked.Exchange(ref _callback, null);
    }
}
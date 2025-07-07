namespace NxRelay
{
    public interface IHandler<in TMessage>
    {
        
        ValueTask HandleAsync(TMessage message, CancellationToken ct);
        bool Filter(TMessage message);
    }
}

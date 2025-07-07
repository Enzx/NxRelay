namespace NxRelay;

public class RelayFilter<TMessage>(Func<TMessage, bool> predicate) : Filter<TMessage>
{
    private readonly Func<TMessage, bool> _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));

    public override bool Apply(TMessage message) => _predicate(message);
}

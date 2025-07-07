namespace NxRelay;

/// <summary>
/// A filter that can be applied to a message handler to determine if the message should be handled.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
public abstract class Filter<TMessage>
{
    public abstract bool Apply(TMessage message);
}
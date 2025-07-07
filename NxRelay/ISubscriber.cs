namespace NxRelay
{
    public interface ISubscriber<TMessage>
    {
        IDisposable Subscribe(IHandler<TMessage> message);
        void Unsubscribe(SubscriptionToken<long> token);
    }
}
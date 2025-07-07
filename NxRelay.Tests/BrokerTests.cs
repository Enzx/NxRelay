namespace NxRelay.Tests;

[TestFixture]
public class BrokerTests
{
    private readonly Broker<string> _broker = new();

    [Test]
    public async Task PublishInvokesSubscribedHandler()
    {
        string? received = null;
        IDisposable token = _broker.Subscribe(new Handler<string>(msg => received = msg));
        await _broker.Publish("hello");
        Assert.That(received, Is.EqualTo("hello"));
        token.Dispose();
    }

    [Test]
    public async Task PublishAfterUnsubscribeDoesNotInvoke()
    {
        bool called = false;
        IDisposable token = _broker.Subscribe(new Handler<string>(_ => called = true));
        token.Dispose();
        await _broker.Publish("ignored");
        Assert.That(called, Is.False);
    }

    [Test]
    public Task PublishWithWrongTypeThrows()
    {
        return Assert.ThrowsAsync<ArgumentException>(async () => await _broker.Publish(42));
    }
}

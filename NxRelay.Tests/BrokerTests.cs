namespace NxRelay.Tests;

[TestFixture]
public class BrokerTests
{
    private readonly Broker<string> _broker = new();

    [Test]
    public async Task PublishInvokesSubscribedHandler()
    {
        string? received = null;
        IAsyncDisposable token = _broker.Subscribe(new Handler<string>(msg => received = msg));
        await _broker.Publish("hello");
        Assert.That(received, Is.EqualTo("hello"));
        await token.DisposeAsync();
    }

    [Test]
    public async Task PublishAfterUnsubscribeDoesNotInvoke()
    {
        bool called = false;
        IAsyncDisposable token = _broker.Subscribe(new Handler<string>(_ => called = true));
        await token.DisposeAsync();
        await _broker.Publish("ignored");
        Assert.That(called, Is.False);
    }

    [Test]
    public Task PublishWithWrongTypeThrows()
    {
        Assert.ThrowsAsync<ArgumentException>(async () => await _broker.Publish(42));
        return Task.CompletedTask;
    }
}
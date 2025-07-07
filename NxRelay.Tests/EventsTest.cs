namespace NxRelay.Tests;

[TestFixture]
public class EventsTest
{
    private readonly Events _events = new();
    
    [Test]
    public void TestSubscribeAndPublish()
    {
        bool wasCalled = false;
        _events.Subscribe<string>(message => wasCalled = true);
        
        _ = _events.Publish("Test Message");
        
        Assert.That(wasCalled, Is.True, "Event handler should have been called.");
    }
    
    [Test]
    public void TestSubscribeWithFilterAndPublish()
    {
        bool wasCalled = false;
        RelayFilter<string> filter = new(message => message.Contains("Test"));
        
        _events.Subscribe(_ => wasCalled = true, filter);
        
        _ = _events.Publish("Test Message");
        
        Assert.That(wasCalled, Is.True, "Event handler should have been called with filtered message.");
        
        wasCalled = false;
        _ = _events.Publish("Another Message");
        
        Assert.That(wasCalled, Is.False, "Event handler should not have been called with non-matching message.");
    }
    
    [Test]
    public void TestSubscribeWithMultipleFiltersAndPublish()
    {
        bool wasCalled = false;
        RelayFilter<string> filter1 = new(message => message.Contains("Test"));
        RelayFilter<string> filter2 = new(message => message.Length > 5);
        
        _events.Subscribe(_ => wasCalled = true, filter1, filter2);
        
        _ = _events.Publish("Test Message");
        
        Assert.That(wasCalled, Is.True, "Event handler should have been called with multiple filters.");
        
        wasCalled = false;
        _ = _events.Publish("Short");
        
        Assert.That(wasCalled, Is.False, "Event handler should not have been called with non-matching message.");
    }
    
    [Test]
    public void TestSubscribeWithActionAndPublish()
    {
        bool wasCalled = false;
        _events.Subscribe<string>(message => wasCalled = true);
        
        _ = _events.Publish("Action Test Message");
        
        Assert.That(wasCalled, Is.True, "Event handler with action should have been called.");
    }
    
    [Test]
    public void TestUnsubscribe()
    {
        bool wasCalled = false;
        IDisposable subscription = _events.Subscribe<string>(message => wasCalled = true);
        
        subscription.Dispose();
        
        _ = _events.Publish("Test Message After Unsubscribe");
        
        Assert.That(wasCalled, Is.False, "Event handler should not have been called after unsubscribe.");
    }
    
    [Test]
    public void TestPublishNullMessageThrowsException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _events.Publish<string>(null!),
            "Publishing a null message should throw an ArgumentNullException.");
    }
}
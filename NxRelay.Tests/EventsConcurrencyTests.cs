using System.Diagnostics;

namespace NxRelay.Tests;

[TestFixture]
public class EventsConcurrencyTests
{
    [Test]
    public async Task Publish_IsThreadSafe()
    {
        Events events = new();
        int counter = 0;
        IAsyncDisposable dispose = events.Subscribe<int>(
            i =>
            {
                counter = Interlocked.Increment(ref counter);
            });

        IEnumerable<Task<bool>> tasks = new List<Task<bool>>(10_000);
        for (int i = 0; i < 10_000; i++)
        {
            tasks = tasks.Append(events.Publish(i));
        }
        
        await Task.WhenAll(tasks);
        await dispose.DisposeAsync();
        Assert.That(counter, Is.EqualTo(10_000));
    }
}
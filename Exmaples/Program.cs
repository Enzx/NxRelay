// See https://aka.ms/new-console-template for more information

using NxRelay;

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
Console.WriteLine($"Counter reached: {counter}");
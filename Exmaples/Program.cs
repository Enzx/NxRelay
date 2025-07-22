// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using NxRelay;

namespace Exmaples;

internal static class Program
{
    public static async Task<int> Main(string[] args)
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
        Console.WriteLine($"Counter reached: {counter}");
        ServiceProvider sp = new ServiceCollection().BuildServiceProvider();

        Mediator mediator = new(sp);
        mediator.Register(new MyRequestHandler());
        Response response = await mediator.Send(new Request("Hello, World!"));
        Console.WriteLine($"Response: {response.Content}");
        return 0;
    }
}

public readonly struct Request(string content) : IRequest<Response>
{
    public string Content { get; } = content;
}

public readonly struct Response(string content) 
{
    public string Content { get; } = content;
}
public class MyRequestHandler : IRequestHandler<Request, Response>  
{
    public ValueTask<Response> Handle(Request request, CancellationToken ct)
    {
        // Simulate some processing
        return new ValueTask<Response>(new Response($"Processed: {request.Content}"));
    }
}
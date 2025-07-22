namespace NxRelay.Tests;

[TestFixture(Description = "Tests for Mediator functionality", Category = "Mediator", TestOf = typeof(Mediator))]
public class MediatorTests
{
    private class EventHandler : IRequestHandler<Request, string>
    {
        public async ValueTask<string> Handle(Request request, CancellationToken ct)
        {
            await Task.Delay(1, ct);
            return $"Processed: {request.Message}";
        }
    }

    private class Request : IRequest<string>
    {
        public string Message { get; init; } = string.Empty;
    }

    private class UnregisteredRequest : IRequest<string>
    {
        public string Message { get; init; } = string.Empty;
    }

    private readonly EventHandler _eventHandler = new();
    private Mediator _mediator;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mediator();
    }

    [Test]
    public async Task TestMediatorHandlesRequest()
    {
        _mediator.Register(_eventHandler);
        Request request = new() { Message = "Hello, World!" };
        string response = await _mediator.Send(request);
        Assert.That(response, Is.EqualTo("Processed: Hello, World!"));
    }

    [Test]
    public void TestMediatorThrowsOnUnregisteredRequest()
    {
        UnregisteredRequest unregisteredRequest = new() { Message = "This will fail" };
        InvalidOperationException? ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _mediator.Send(unregisteredRequest));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo($"No handler for {nameof(UnregisteredRequest)}"));
    }

    [Test]
    public void TestRegisterDuplicateHandlerThrows()
    {
        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(
            () => _mediator.Register(_eventHandler));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo($"Handler already registered for {nameof(Request)}"));
    }

    [TearDown]
    public void TearDown()
    {
        _mediator.Dispose();
    }
}
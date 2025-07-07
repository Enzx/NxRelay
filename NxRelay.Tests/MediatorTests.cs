namespace NxRelay.Tests;

[TestFixture(Description = "Tests for Mediator functionality", Category = "Mediator", TestOf = typeof(Mediator))]
public class MediatorTests
{
    private class EventHandler : IRequestHandler<Request, string>
    {
        public async ValueTask<string> HandleAsync(Request request, CancellationToken ct)
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
    private readonly Mediator _mediator = new();

    [SetUp]
    public void Setup()
    {
        _mediator.Register(_eventHandler);
    }

    [Test]
    public async Task TestMediatorHandlesRequest()
    {
        Request request = new() { Message = "Hello, World!" };
        string response = await _mediator.SendAsync<Request, string>(request);
        Assert.That(response, Is.EqualTo("Processed: Hello, World!"));
    }

    [Test]
    public void TestMediatorThrowsOnUnregisteredRequest()
    {
        UnregisteredRequest unregisteredRequest = new() { Message = "This will fail" };
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _mediator.SendAsync<UnregisteredRequest, string>(unregisteredRequest));
    }

    [Test]
    public void TestRegisterDuplicateHandlerThrows()
    {
        Assert.Throws<InvalidOperationException>(() => _mediator.Register(_eventHandler));
    }

    [TearDown]
    public void TearDown()
    {
        _mediator.Dispose();
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace NxRelay.Tests;

[TestFixture]
public class MessagingServiceCollectionExtensionsTests
{
    private class TestRequest : IRequest<string>
    {
        public string Message { get; init; } = string.Empty;
    }

    private class TestHandler : IRequestHandler<TestRequest, string>
    {
        public ValueTask<string> HandleAsync(TestRequest request, CancellationToken ct)
        {
            return new ValueTask<string>($"handled:{request.Message}");
        }
    }

    [Test]
    public void AddMessaging_RegistersHandlersFromSpecifiedAssemblies()
    {
        ServiceCollection services = new();
        services.AddMessaging(typeof(TestHandler).Assembly);
        ServiceProvider provider = services.BuildServiceProvider();
        IRequestHandler<TestRequest, string>? handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.That(handler, Is.Not.Null);
    }
}

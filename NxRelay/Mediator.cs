using System.Collections.Concurrent;

namespace NxRelay;

/// <summary>
/// Simple mediator for dispatching request/response messages.
/// </summary>
public sealed class Mediator : IDisposable, IMediator
{
    private readonly ConcurrentDictionary<Type, IRequestHandler> _requestHandlers = new();

    /// <summary>
    /// Registers a handler for a request type. Only one handler per request is allowed.
    /// </summary>
    public void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        if (!_requestHandlers.TryAdd(typeof(TRequest), handler))
            throw new InvalidOperationException($"Handler already registered for {typeof(TRequest).Name}");
    }

    /// <summary>
    /// Dispatches the request to the registered handler and returns its response.
    /// </summary>
    public ValueTask<TResponse> Send<TRequest, TResponse>(
        TRequest request, CancellationToken ct = default)
        where TRequest : IRequest<TResponse>
    {
        if (_requestHandlers.TryGetValue(typeof(TRequest), out IRequestHandler? h) &&
            h is IRequestHandler<TRequest, TResponse> handler)
            return handler.Handle(request, ct);
        throw new InvalidOperationException($"No handler for {typeof(TRequest).Name}");
    }

    public void Dispose()
    {
        _requestHandlers.Clear();
    }
}
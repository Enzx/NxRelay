using System.Collections.Concurrent;

namespace NxRelay;

/// <summary>
/// Simple mediator for dispatching request/response messages.
/// </summary>
public class Mediator : IDisposable, IMediator
{
    private readonly ConcurrentDictionary<Type, object> _requestHandlers = new();
    
    /// <summary>
    /// Registers a handler for a request type. Only one handler per request is allowed.
    /// </summary>
    public void Register<TRequest,TResponse>(IRequestHandler<TRequest,TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        if (!_requestHandlers.TryAdd(typeof(TRequest), handler))
            throw new InvalidOperationException(
                $"Handler already registered for {typeof(TRequest).Name}");
    }
    
    /// <summary>
    /// Dispatches the request to the registered handler and returns its response.
    /// </summary>
    public ValueTask<TResponse> SendAsync<TRequest,TResponse>(
        TRequest request, CancellationToken ct = default)
        where TRequest : IRequest<TResponse>
    {
        if (_requestHandlers.TryGetValue(typeof(TRequest), out var obj) &&
            obj is IRequestHandler<TRequest,TResponse> h)
        {
            return h.HandleAsync(request, ct);
        }
        throw new InvalidOperationException(
            $"No handler for {typeof(TRequest).Name}");
    }

    public void Dispose() => _requestHandlers.Clear();
}

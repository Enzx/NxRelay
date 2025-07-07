using System.Collections.Concurrent;

namespace NxRelay;

public class Mediator : IDisposable, IMediator
{
    private readonly ConcurrentDictionary<Type, object> _requestHandlers = new();
    
    public void Register<TRequest,TResponse>(IRequestHandler<TRequest,TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        if (!_requestHandlers.TryAdd(typeof(TRequest), handler))
            throw new InvalidOperationException(
                $"Handler already registered for {typeof(TRequest).Name}");
    }
    
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
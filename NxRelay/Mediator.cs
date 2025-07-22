using System.Collections.Concurrent;

namespace NxRelay;

/// <summary>
/// Simple mediator for dispatching request/response messages.
/// </summary>
public sealed class Mediator : IDisposable, IMediator
{
    private readonly ConcurrentDictionary<Type, IHandlerWrapper> _requestHandlers = new();

    /// <summary>
    /// Registers a handler for a request type. Only one handler per request is allowed.
    /// </summary>
    public void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        Type requestType = typeof(TRequest);
        if (!_requestHandlers.TryAdd(requestType, new HandlerWrapper<TRequest, TResponse>(handler)))
            throw new InvalidOperationException($"Handler already registered for {typeof(TRequest).Name}");
    }

    /// <summary>
    /// Dispatches the request to the registered handler and returns its response.
    /// </summary>
    public ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct = default)
    {
        Type requestType = request.GetType();
        if (_requestHandlers.TryGetValue(requestType, out IHandlerWrapper? wrapperObj))
            return ((IHandlerWrapper<TResponse>)wrapperObj)
                .Handle(request, ct);

        throw new InvalidOperationException(
            $"No handler for {requestType.Name}");
    }

    public void Dispose()
    {
        _requestHandlers.Clear();
    }
}
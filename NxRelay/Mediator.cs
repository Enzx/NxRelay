using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace NxRelay;

/// <summary>
/// Simple mediator for dispatching request/response messages.
/// </summary>
public sealed class Mediator : IDisposable, IMediator
{
    private readonly IServiceProvider _sp;
    private readonly ConcurrentDictionary<Type, IHandlerWrapper> _requestHandlers = new();

    public Mediator(IServiceProvider sp) => _sp = sp;
    

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
        IHandlerWrapper<TResponse> wrapper = (IHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(requestType,
            static (t, state) =>
            {
                (IServiceProvider sp, Type responseType) = ((IServiceProvider, Type))state!;
                Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(t, responseType);
                object handler = sp.GetRequiredService(handlerType);
                Type wrapperType = typeof(HandlerWrapper<,>).MakeGenericType(t, responseType);
                return (IHandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;
            }, (_sp, typeof(TResponse)));

        return wrapper.Handle(request, ct);

      
    }

    public void Dispose()
    {
        _requestHandlers.Clear();
    }
}
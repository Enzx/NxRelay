namespace NxRelay;

/// <summary>
/// Contract for a mediator that sends requests to registered handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Registers a handler for a specific request type.
    /// </summary>
    void Register<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Sends a request and awaits the response from the registered handler.
    /// </summary>
    ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request, CancellationToken ct = default);
}
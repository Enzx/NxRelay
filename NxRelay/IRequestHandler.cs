namespace NxRelay;

/// <summary>
/// Handler for a request that returns a response.
/// </summary>
public interface IRequestHandler<in TRequest,TResponse>
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

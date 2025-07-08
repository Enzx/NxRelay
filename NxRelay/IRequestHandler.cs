namespace NxRelay;

public interface IRequestHandler
{
    
}

/// <summary>
/// Handler for a request that returns a response.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}
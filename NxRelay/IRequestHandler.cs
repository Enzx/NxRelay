namespace NxRelay;

public interface IRequestHandler<in TRequest,TResponse>
    where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

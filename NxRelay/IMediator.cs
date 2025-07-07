namespace NxRelay;

public interface IMediator
{
    void Register<TRequest,TResponse>(IRequestHandler<TRequest,TResponse> handler)
        where TRequest : IRequest<TResponse>;

    ValueTask<TResponse> SendAsync<TRequest,TResponse>(
        TRequest request, CancellationToken ct = default)
        where TRequest : IRequest<TResponse>;
}
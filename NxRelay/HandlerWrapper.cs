namespace NxRelay;

internal sealed class HandlerWrapper<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler) :
    IHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValueTask<TResponse> Handle(
            IRequest<TResponse> request, CancellationToken ct)
        => handler.Handle((TRequest)request, ct);
}
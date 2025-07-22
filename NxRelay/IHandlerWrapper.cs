namespace NxRelay;

internal interface IHandlerWrapper
{
}

internal interface IHandlerWrapper<TResponse> : IHandlerWrapper
{
    ValueTask<TResponse> Handle(IRequest<TResponse> request, CancellationToken ct);
}
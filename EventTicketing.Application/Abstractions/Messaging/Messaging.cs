namespace EventTicketing.Application.Abstractions.Messaging;

// A request that expects a response of type TResponse.
public interface IRequest<out TResponse> { }

// CQRS markers. A command changes state; a query only reads. The non-generic
// IBaseCommand lets a behavior spot a command without knowing its response type.
public interface IBaseCommand { }
public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand { }
public interface IQuery<out TResponse> : IRequest<TResponse> { }

// Handles exactly one request type and produces its response.
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}

// The next step in the pipeline — either the next behavior or the handler itself.
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

// A decorator that wraps handling: do something, call next(), do something.
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct = default);
}

// Sends a request to its handler — without the caller knowing which handler that is.
public interface ISender
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}

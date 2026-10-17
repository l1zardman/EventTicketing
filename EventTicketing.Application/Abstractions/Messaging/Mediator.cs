using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketing.Application.Abstractions.Messaging;

// The mediator, now with a behavior pipeline. It hops once through reflection to
// recover the concrete request type, then everything is strongly typed: resolve
// the handler, wrap it in each registered behavior (outermost first), and run.
public sealed class Mediator : ISender
{
    private static readonly MethodInfo SendInternalMethod =
        typeof(Mediator).GetMethod(nameof(SendInternal), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private readonly IServiceProvider _provider;
    public Mediator(IServiceProvider provider) => _provider = provider;

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var method = SendInternalMethod.MakeGenericMethod(request.GetType(), typeof(TResponse));
        return (Task<TResponse>)method.Invoke(this, new object?[] { request, ct })!;
    }

    private Task<TResponse> SendInternal<TRequest, TResponse>(TRequest request, CancellationToken ct)
        where TRequest : IRequest<TResponse>
    {
        var handler = _provider.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

        RequestHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(request, ct);

        // Wrap in reverse so the first-registered behavior ends up outermost.
        foreach (var behavior in _provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var next = pipeline;
            pipeline = () => behavior.HandleAsync(request, next, ct);
        }

        return pipeline();
    }
}

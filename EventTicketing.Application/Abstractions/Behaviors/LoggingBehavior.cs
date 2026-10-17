using EventTicketing.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace EventTicketing.Application.Abstractions.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct = default)
    {
        var name = typeof(TRequest).Name;
        _logger.LogInformation("Handling {Request}", name);
        try
        {
            var response = await next();
            _logger.LogInformation("Handled {Request}", name);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Request} failed", name);
            throw;
        }
    }
}

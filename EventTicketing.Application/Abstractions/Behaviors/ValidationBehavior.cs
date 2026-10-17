using EventTicketing.Application.Abstractions.Messaging;
using FluentValidation;

namespace EventTicketing.Application.Abstractions.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct = default)
    {
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(request, ct);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await next();
    }
}

using EventTicketing.Application.Abstractions.Messaging;
using EventTicketing.Domain.Common;

namespace EventTicketing.Application.Abstractions.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    public UnitOfWorkBehavior(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct = default)
    {
        if (request is not IBaseCommand)
            return await next();

        var response = await next();

        // A failed command has nothing to persist — don't commit.
        if (response is Result { IsFailure: true })
            return response;

        await _unitOfWork.SaveChangesAsync(ct);
        return response;
    }
}

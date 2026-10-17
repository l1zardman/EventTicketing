using FluentValidation;

namespace EventTicketing.Application.Events;

// Input validation: rejects malformed requests early with a clean message.
// (The domain still guards capacity as the final authority — belt and suspenders.)
public sealed class ReserveTicketsCommandValidator : AbstractValidator<ReserveTicketsCommand>
{
    public ReserveTicketsCommandValidator()
    {
        RuleFor(c => c.EventId).GreaterThan(0);
        RuleFor(c => c.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}

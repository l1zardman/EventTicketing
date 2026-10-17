namespace EventTicketing.Domain.Common;

// Thrown when an invariant is violated — i.e. when code tries to force the
// domain into a state that must never exist.
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

using System.Linq.Expressions;

namespace EventTicketing.Domain.Common;

// A query rule as a first-class object: reusable, nameable, unit-testable, and
// translatable to SQL by EF because it exposes an expression, not executed code.
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T candidate) => ToExpression().Compile()(candidate);
}

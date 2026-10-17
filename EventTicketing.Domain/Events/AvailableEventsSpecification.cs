using System.Linq.Expressions;
using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Events;

// "An event with at least one ticket left." One rule, one class, used anywhere.
public sealed class AvailableEventsSpecification : Specification<Event>
{
    public override Expression<Func<Event, bool>> ToExpression()
        => e => e.TicketsSold < e.Capacity;
}

using EventTicketing.Domain.Common;

namespace EventTicketing.Domain.Events;

public static class EventErrors
{
    public static Error NotFound(int eventId) =>
        Error.NotFound("Event.NotFound", $"Event {eventId} was not found.");

    public static Error InsufficientCapacity(int available, int requested) =>
        Error.Conflict("Event.InsufficientCapacity",
            $"Not enough tickets: {available} available, {requested} requested.");

    public static Error TooManyToRelease(int sold, int requested) =>
        Error.Conflict("Event.TooManyToRelease",
            $"Cannot release {requested} tickets; only {sold} are sold.");
}

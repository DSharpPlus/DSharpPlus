namespace DSharpPlus.Interactivity;
using Microsoft.Extensions.Logging;

/// <summary>
/// Contains well-defined event IDs used by the Interactivity extension.
/// </summary>
public static class InteractivityEvents
{
    /// <summary>
    /// Miscellaneous events, that do not fit in any other category.
    /// </summary>
    public static EventId Misc { get; } = new EventId(500, "Interactivity");

    /// <summary>
    /// Events pertaining to errors that happen during waiting for events.
    /// </summary>
    public static EventId InteractivityWaitError { get; } = new EventId(501, nameof(InteractivityWaitError));

    /// <summary>
    /// Events pertaining to pagination.
    /// </summary>
    public static EventId InteractivityPaginationError { get; } = new EventId(502, nameof(InteractivityPaginationError));

    /// <summary>
    /// Events pertaining to polling.
    /// </summary>
    public static EventId InteractivityPollError { get; } = new EventId(503, nameof(InteractivityPollError));

    /// <summary>
    /// Events pertaining to event collection.
    /// </summary>
    public static EventId InteractivityCollectorError { get; } = new EventId(504, nameof(InteractivityCollectorError));
}

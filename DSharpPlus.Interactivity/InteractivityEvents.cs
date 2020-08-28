using Microsoft.Extensions.Logging;

namespace DSharpPlus.Interactivity
{
    internal static class InteractivityEvents
    {
        public static EventId Misc { get; } = new EventId(500, "Interactivity");
        public static EventId InteractivityWaitError { get; } = new EventId(501, nameof(InteractivityWaitError));
        public static EventId InteractivityPaginationError { get; } = new EventId(502, nameof(InteractivityPaginationError));
        public static EventId InteractivityPollError { get; } = new EventId(503, nameof(InteractivityPollError));
        public static EventId InteractivityCollectorError { get; } = new EventId(504, nameof(InteractivityCollectorError));
    }
}

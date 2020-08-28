using Microsoft.Extensions.Logging;

namespace DSharpPlus.CommandsNext
{
    internal static class CommandsNextEvents
    {
        internal static EventId Misc { get; } = new EventId(200, "CommandsNext");
        internal static EventId Intents { get; } = new EventId(201, nameof(Intents));
    }
}

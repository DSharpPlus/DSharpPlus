using Microsoft.Extensions.Logging;

namespace DSharpPlus.CommandsNext
{
    internal static class CommandsNextEvents
    {
        internal static EventId Misc { get; } = new EventId(200, "CommandsNext");
    }
}

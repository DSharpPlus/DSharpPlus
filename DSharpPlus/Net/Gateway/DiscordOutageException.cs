using System;

namespace DSharpPlus.Net.Gateway;

// we ideally want to treat outages transiently via backoff reconnect, so this is internal
internal sealed class DiscordOutageException : Exception
{
    public DiscordOutageException(string message) : base(message)
    {
        
    }
}

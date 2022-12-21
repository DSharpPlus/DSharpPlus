using System;

namespace DSharpPlus.Entities;

[Flags]
public enum DiscordChannelFlags
{
    /// <summary>
    /// This thread is pinned to the top of its parent forum channel.
    /// </summary>
    Pinned = 1 << 1
}

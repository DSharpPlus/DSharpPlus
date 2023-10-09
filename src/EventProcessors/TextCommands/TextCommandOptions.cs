using System;

namespace DSharpPlus.CommandAll.EventProcessors
{
    [Flags]
    public enum TextCommandOptions
    {
        AllowBots = 1 << 0,
        AllowDMs = 1 << 1,
        AllowGuilds = 1 << 2,
    }
}

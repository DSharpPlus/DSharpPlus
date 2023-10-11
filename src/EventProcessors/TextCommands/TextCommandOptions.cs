using System;

namespace DSharpPlus.CommandAll.Processors
{
    [Flags]
    public enum TextCommandOptions
    {
        AllowBots = 1 << 0,
        AllowDMs = 1 << 1,
        AllowGuilds = 1 << 2,
    }
}

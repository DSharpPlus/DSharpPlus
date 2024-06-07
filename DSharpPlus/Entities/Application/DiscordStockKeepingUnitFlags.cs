using System;

namespace DSharpPlus.Entities;

[Flags]
public enum DiscordStockKeepingUnitFlags
{
    Available = 1 << 2,
    GuildSubscription = 1 << 7,
    UserSubscription = 1 << 8
}

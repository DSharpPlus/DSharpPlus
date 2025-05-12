using System;
using DSharpPlus.Entities;

namespace DSharpPlus.Hosting;

public class DiscordClientStartupOptions
{
    public DiscordActivity? Activity { get; set; }

    public DiscordUserStatus? Status { get; set; }

    public DateTimeOffset? IdleSince { get; set; }
}

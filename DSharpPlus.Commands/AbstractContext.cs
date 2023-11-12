namespace DSharpPlus.Commands;

using System;
using DSharpPlus.Commands.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

public abstract record AbstractContext
{
    public required DiscordUser User { get; init; }
    public required DiscordChannel Channel { get; init; }
    public required CommandsExtension Extension { get; init; }
    public required Command Command { get; init; }
    public required AsyncServiceScope ServiceScope { internal get; init; }

    public DiscordGuild? Guild => this.Channel.Guild;
    public DiscordMember? Member => this.User as DiscordMember;
    public DiscordClient Client => this.Extension.Client;
    public IServiceProvider ServiceProvider => this.ServiceScope.ServiceProvider;
}

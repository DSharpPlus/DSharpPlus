namespace DSharpPlus.Commands;

using System;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

public abstract record AbstractContext
{
    public required DiscordUser User { get; init; }
    public required DiscordChannel Channel { get; init; }
    public required CommandsExtension Extension { get; init; }
    public required Command Command { get; init; }
    public required IServiceScope ServiceScope { internal get; init; }

    public DiscordGuild? Guild => Channel.Guild;
    public DiscordMember? Member => User as DiscordMember;
    public DiscordClient Client => Extension.Client;
    public IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;
}

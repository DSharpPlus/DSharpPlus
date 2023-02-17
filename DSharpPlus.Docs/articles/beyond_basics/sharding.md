---
uid: articles.beyond_basics.sharding
title: Sharding
---

# Sharding
As your bot joins more guilds, your poor @DSharpPlus.DiscordClient will be hit with an increasing number of events.
Thankfully, Discord allows you to establish multiple connections to split the event workload; this is called *sharding*
and each individual connection is referred to as a *shard*. Each shard handles a separate set of servers and will *only*
receive events from those servers. However, all direct messages will be handled by your first shard.

Sharding is recommended once you reach 1,000 servers, and is a *requirement* when you hit 2,500 servers.

## Automated Sharding
DSharpPlus provides a built-in sharding solution: @DSharpPlus.DiscordShardedClient. This client will *automatically*
spawn shards for you and manage their events. Each DSharpPlus extension (e.g. CommandsNext, Interactivity) also supplies
an extension method to register themselves automatically on each shard.
```cs
var discord = new DiscordShardedClient(new DiscordConfiguration
{
    Token = "My First Token",
    TokenType = TokenType.Bot
});

await discord.UseCommandsNextAsync(new CommandsNextConfiguration()
{
    StringPrefixes = new[] { "!" }
});
```

## Manual Sharding 
For most looking to shard, the built-in @DSharpPlus.DiscordShardedClient will work well enough. However, those looking
for more control over the sharding process may want to handle it manually.

This would involve creating new @DSharpPlus.DiscordClient instances, assigning each one an appropriate shard ID number,
and handling the events from each instance. Considering the potential complexity imposed by this process, you should
only do this if you have a valid reason to do so and *know what you are doing*.
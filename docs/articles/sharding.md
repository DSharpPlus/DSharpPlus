# Sharding - for when your bot gets huge

Sometimes, your bot becomes very large. More and more servers add it. Eventually, you will need to split the bot's workload 
a bit.

## What is sharding?

In Discord, sharding describes a situation in which your bot initiates several connections to Discord. This is recommended for 
bots that grow over 1000 guilds, and required if you go over 2500.

Each shard gets a different set of guilds, and they only get events appropriate to guilds on their shard.

## How do I shard?

In DSharpPlus there are 2 ways to shard. The easy way - using @DSharpPlus.DiscordShardedClient, or the hard way, spawning 
multiple @DSharpPlus.DiscordClient instances manually.

### The easy way - DiscordShardedClient

`DiscordShardedClient` will automatically handle spawning, handling and controlling the appropriate amount of shards for your 
bot. Additionally, all the modules offer extensions that enable the modules on all of the sharded client's shards automatically.

Sharded client is fairly transparent when it comes to handling events, and since all events expose the `DiscordClient` that 
emitted it, you can identify which shard did the event came from.

### The hard way - multiple DiscordClient instances

This is basically operating much like `DiscordShardedClient` does internally, but you have to manually handle all the events 
and shards.

This approach offers more control over your shards than the sharded client does, but it recommended for experienced developers 
only.

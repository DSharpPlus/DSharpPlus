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

DSharpPlus provides a built-in sharding solution: `DiscordClient`. `DiscordClientBuilder` and the service collection configuration both offer ways to configure a sharding DiscordClient:

# [DiscordClientBuilder](#tab/discordclientbuilder)

```cs
DiscordClientBuilder builder = DiscordClientBuilder.CreateSharded("My First Token", DiscordIntents.All);
DiscordClient shardingClient = builder.Build();
```

# [IServiceCollection](#tab/iservicecollection)

```cs
serviceCollection.AddShardedDiscordClient("My First Token", DiscordIntents.All);
```

---

## Further Customization

For most looking to shard, the built-in sharded client will work well enough. However, those looking for more control over the sharding process may want to handle it manually. The default `MultiShardOrchestrator` provides the ability to only start a certain set of the total shards within the current `DiscordClient` via setting the `Stride` and `TotalShards` properties:

```cs
serviceCollection.Configure<ShardingOptions>(x => 
{
    x.Stride = 16;
    x.ShardCount = 16;
    x.TotalShards = 32;
});
```

Furthermore, it is possible to override the orchestrator entirely and replace it with your own, which can then do whatever you want:

```cs
public class MyCustomOrchestrator : IShardOrchestrator;

serviceCollection.AddSingleton<IShardOrchestrator, MyCustomOrchestrator>();
```

Your orchestrator will need to be able to connect, reconnect and disconnect, it will need to be able to send payloads to Discord and expose whether a certain shard is connected correctly. By default, each shard is represented as an `IGatewayClient`, but if you are writing your own orchestrator, you are free to implement the individual shards yourself.

Implementing a custom orchestrator should be done as a last resort, when you have verified the default implementations cannot do anything for you nor can be adapted to work for you and when you are entirely sure you understand sharding to a degree where you would be able to implement your own orchestrator.

Additionally, if you have a very large bot, the default `MultiShardOrchestrator` may not meet your needs. In that case, please review the documentation and all material Discord has provided you with carefully and consider implementing your own `IShardOrchestrator` or reaching out to us to find a solution.

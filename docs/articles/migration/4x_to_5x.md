---
uid: articles.migration.4x_to_5x
title: Migration 4.x - 5.x
---

## Migration from 4.x to 5.x

> [!NOTE]
> The API surface of DSharpPlus 5.x is not stable yet. This migration guide may be incomplete or outdated. It is recommended to cross-reference with the [tracking issue](https://github.com/DSharpPlus/DSharpPlus/issues/1585) when migrating.

The first change you will likely encounter is a rewrite to how bots are set up. We now support two approaches instead of the old approach:

# [DiscordClientBuilder](#tab/discordclientbuilder)

The simplest way to get a bot running is to use `DSharpPlus.DiscordClientBuilder`. To get started, create a new builder as follows: 

```cs
DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(string token, DiscordIntents intents);
```

Instead, if you are sharding, create it as follows:

```cs
DiscordClientBuilder builder = DiscordClientBuilder.CreateSharded(string token, DiscordIntents intents, uint? shardCount);
```

You may omit the shard count to instead defer to Discord's recommended shard count.

Then, migrate your configuration options. Rest-related settings from your old DiscordConfiguration are covered by `DiscordClientBuilder.ConfigureRestClient`, gateway-related settings are covered by `DiscordClientBuilder.ConfigureGatewayClient`.

`LogLevel` has been migrated to `DiscordClientBuilder.SetLogLevel`, and configuring the gateway client is now done through either overriding or decorating the default client via `DiscordClientBuilder.ConfigureServices`. It is comprised of two parts, `ITransportService` and `IGatewayClient`

Then, we will need to update event handling. For more information, see [the dedicated article](../beyond_basics/events.md), but in short, events have also been migrated to DiscordClientBuilder: events are now handled through `DiscordClientBuilder.ConfigureEventHandlers`. You can register handlers on the configuration delegate as follows:

```cs
builder.ConfigureEventHandlers
(
    b => b.HandleMessageCreated(async (s, e) => 
    {
        if (e.Message.Content.ToLower().StartsWith("spiderman"))
        {
            await e.Message.RespondAsync("I want pictures of Spiderman!");
        }
    })
    .HandleGuildMemberAdded(OnGuildMemberAdded)
);

private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddedEventArgs args)
{
    // non-asynchronous code here
    return Task.CompletedTask;
}
```

Lastly, we'll need to migrate our extensions. Our extensions have an `UseExtensionName` method for DiscordClientBuilder, similar to how they previously had such extensions for DiscordClient. Some extensions take an additional `Action<ExtensionType>` parameter you can use to configure the extension, like so:

```cs
builder.UseCommandsExtension
(
    extension =>
    {
        extension.AddCommands([typeof(MyCommands)]);
        extension.AddParameterCheck(typeof(MyCheck))
    }
);
```

# [IServiceCollection](#tab/iservicecollection)

If you need more advanced setup than DiscordClientBuilder facilitates, you can register DSharpPlus against an IServiceCollection.

First, register all necessary services:

```cs
serviceCollection.AddDiscordClient(string token, DiscordIntents intents);
```

Alternatively, if you are sharding, register them as such:

```cs
serviceCollection.AddShardedDiscordClient(string token, DiscordIntents intents);
```

Then, migrate your configuration options to calls to `serviceCollection.Configure<RestClientOptions>();`, `serviceCollection.Configure<GatewayClientOptions>();` and `serviceCollection.Configure<ShardingOptions>();`, respectively. `DiscordConfiguration` is a valid target to configure, however it only contains a few remaining configuration knobs not covered by the other configurations.

When registering against a service collection, you are expected to provide your own logging setup, and DSharpPlus' default logging will not be registered.

To handle events, use the extension method `ConfigureEventHandlers`:

```cs
services.ConfigureEventHandlers
(
    b => b.HandleMessageCreated(async (s, e) => 
    {
        if (e.Message.Content.ToLower().StartsWith("spiderman"))
        {
            await e.Message.RespondAsync("I want pictures of Spiderman!");
        }
    })
    .HandleGuildMemberAdded(OnGuildMemberAdded)
);

private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddedEventArgs args)
{
    // non-asynchronous code here
    return Task.CompletedTask;
}
```

To register extensions, use their extension methods on IServiceCollection. These methods are named `AddXExtension` and accept a configuration object and optionally an `Action<ExtensionType>` for further configuration:

```cs
services.AddCommandsExtension
(
    extension =>
    {
        extension.AddCommands([typeof(MyCommands)]);
        extension.AddParameterCheck(typeof(MyCheck));
    },
    new CommandsConfiguration()
    {
        // ...
    }
);
```

---

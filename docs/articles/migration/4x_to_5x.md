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

Then, migrate your configuration options. Rest-related settings from your old DiscordConfiguration are covered by `DiscordClientBuilder.ConfigureRestClient`, gateway-related settings are covered by `DiscordClientBuilder.ConfigureGatewayClient`.

`LogLevel` has been migrated to `DiscordClientBuilder.SetLogLevel`, and configuring the websocket client is now done through either overriding or decorating the default client via `DiscordClientBuilder.ConfigureServices`. 

Lastly, we will need to update event handling. For more information, see [the dedicated article](../beyond_basics/events), but in short, events have also been migrated to DiscordClientBuilder.

> [!IMPORTANT]
> The ability to handle events from DiscordClient will be removed entirely in a future nightly build, and today, it is no longer possible to unregister events.

Events are now handled through `DiscordClientBuilder.ConfigureEventHandlers`. You can register handlers on the configuration delegate as follows:

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

# [IServiceCollection](#tab/iservicecollection)

If you need more advanced setup than DiscordClientBuilder facilitates, you can register DSharpPlus against an IServiceCollection.

First, register all necessary services:

```cs
serviceCollection.AddDiscordClient(string intents, DiscordIntents intents);
```

Then, migrate your configuration options to calls to `IServiceCollection.Configure<RestClientOptions>();` and `IServiceCollection.Configure<DiscordConfiguration>();`.

> [!NOTE]
> Old rest-related settings on DiscordConfiguration do not work when specified on DiscordConfiguration, and they are only currently retained as glue code for DiscordShardedClient.

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

---

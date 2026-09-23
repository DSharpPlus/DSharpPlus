---
uid: articles.basics.events
title: DSharpPlus Events
---

# Consuming Events

DSharpPlus makes use of *asynchronous events* which will execute each handler asynchronously and in parallel. This event system will require event handlers have a `Task` return type and take two parameters.

The first parameter will contain the active `DiscordClient` associated with the event. The second parameter will contain an arguments object for the specific event you're handling.

Below is a snippet demonstrating this with a lambda expression.

```cs
private async Task Main(string[] args)
{
    DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault( /* token and intents */ );

    builder.ConfigureEventHandlers
    (
        b => b.HandleMessageCreated(async (s, e) => 
        {
            if (e.Message.Content.ToLower().StartsWith("spiderman"))
            {
                await e.Message.RespondAsync("I want pictures of Spiderman!");
            }
        })
        .HandleGuildMemberAdded((s, e) =>
        {
            // non-asynchronous code here
            return Task.CompletedTask;
        })
    );

    DiscordClient client = builder.Build();
}
```

Alternatively, you can create a new method to consume an event.

```cs
private async Task Main(string[] args)
{
    DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault( /* token and intents */ );

    builder.ConfigureEventHandlers
    (
        b => b.HandleMessageCreated(MessageCreatedHandler)
              .HandleGuildMemberAdded(MemberAddedHandler)
    );
}

private async Task MessageCreatedHandler(DiscordClient s, MessageCreatedEventArgs e)
{
    if (e.Guild?.Id == 379378609942560770 && e.Author.Id == 168548441939509248)
    {
        await e.Message.DeleteAsync();
    }
}

private Task MemberAddedHandler(DiscordClient s, GuildMemberAddedEventArgs e)
{
    // Non asynchronous code here.
    return Task.CompletedTask;
}
```

Furthermore, DSharpPlus supports using types as event handlers. These types can participate in dependency injection and will have a respected service lifetime. All you need to do is implement `IEventHandler<TEventArgs>` and enlighten the builder about your event handler:

```cs
public class MyEventHandler : IEventHandler<GuildMemberAddedEventArgs>
{
    // ...
}

DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(token, intents);
builder.ConfigureEventHandlers(b => b.AddEventHandlers<MyEventHandler>(ServiceLifetime.Singleton));
```

One event handler type may handle as many events as you want, simply implement the interface multiple times:

```cs
public class MyEventHandler : IEventHandler<GuildMemberAddedEventArgs>, IEventHandler<GuildMemberRemovedEventArgs>
```

## Usage of the right events

We advise against the use of the `SessionCreated`, as it does not necessarily mean that the client is ready for use. If the goal is to obtain `DiscordMember`/`DiscordGuild` information, this event should not be used. Instead, the `GuildDownloadCompleted` event should be used. The `SessionCreated` event is only meant to signal that a shard has finished the initial handshake with the gateway and is prepared to begin sending payloads.

If all you need is an event for when DSharpPlus begins its startup process and starts interacting with the API, use `ClientStarted`. `ClientStarted` is fired precisely once when starting, even in the face of multiple shards, and is a consistent point for startup code.

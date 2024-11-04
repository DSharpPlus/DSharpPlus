---
uid: articles.beyond_basics.events
title: DSharpPlus Events
---

# Consuming Events

DSharpPlus makes use of *asynchronous events* which will execute each handler asynchronously and in parallel. This
event system will require event handlers have a `Task` return type and take two parameters.

The first parameter will contain an instance of the object which fired the event. The second parameter will contain an
arguments object for the specific event you're handling.

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

We advise against the use of the `SessionCreated`, as it does not necessarily mean that the client
is ready for use. If the goal is to obtain  `DiscordMember`/`DiscordGuild` information, this event should not be used. Instead,
the `GuildDownloadCompleted` event should be used. The `SessionCreated` event is only meant to signal that the client has
finished the initial handshake with the gateway and is prepared to begin sending payloads.

## Migrating to parallel events

In D#+ v4.4.0, events were changed from executing sequentially (each event runs its registered handlers one by one) to
executing in parallel (each event throws all its handlers onto the thread pool). This change has a few benefits, from
mitigating deadlocks previously occurring with certain interactivity-commandsnext interactions to allowing EventArgs
objets to be garbage collected sooner.

For end users, this change should not cause any problems, **unless:**
- **IF** you previously had an event handler for `ComponentInteractionCreated` that indiscriminately responded to all
   interactions while also using button interactivity, your code will break. Make sure you only respond to events you
   actually handle.
- **IF** you previously had two different event handlers on the same event relying on one completing before the other,
   your code will break. Either register only one event handler dealing with all your logic, or manage state yourself.

This change also means that there is no longer a timeout on event handlers, and your event handler is free to take however
long it needs to. There is no longer a reason to wrap your events in a `_ = Task.Run(async () => // logic);`.

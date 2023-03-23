---
uid: articles.beyond_basics.events
title: DSharpPlus Events
---

# Consuming Events

DSharpPlus makes use of *asynchronous events* which will execute each handler asynchronously in sequential order. This
event system will require event handlers have a `Task` return type and take two parameters.

The first parameter will contain an instance of the object which fired the event. The second parameter will contain an
arguments object for the specific event you're handling.

Below is a snippet demonstrating this with a lambda expression.

```cs
private async Task Main(string[] args)
{
    var discord = new DiscordClient();

    discord.MessageCreated += async (s, e) =>
    {
        if (e.Message.Content.ToLower().Contains("spiderman"))
            await e.Message.RespondAsync("I want pictures of Spiderman!");
    };

    discord.GuildMemberAdded += (s, e) =>
    {
        // Non asynchronous code here.
        return Task.CompletedTask;
    };
}
```

Alternatively, you can create a new method to consume an event.

```cs
private async Task Main(string[] args)
{
    var discord = new DiscordClient();

    discord.MessageCreated += MessageCreatedHandler;
    discord.GuildMemberAdded += MemberAddedHandler;
}

private async Task MessageCreatedHandler(DiscordClient s, MessageCreateEventArgs e)
{
    if (e.Guild?.Id == 379378609942560770 && e.Author.Id == 168548441939509248)
        await e.Message.DeleteAsync();
}

private Task MemberAddedHandler(DiscordClient s, GuildMemberAddEventArgs e)
{
    // Non asynchronous code here.
    return Task.CompletedTask;
}
```

You should only register or unregister events on startup or on deterministic points in execution: do not change
event handlers based on user input, in commands or anything related unless you have a very good reason.

# Usage of the right events

 We advise against the use of the `Ready` event in the `DiscordClient`, as it does not necessarily mean that the client
 is ready. If the goal is to obtain  `DiscordMember`/`DiscordGuild` information, this event should not be used. Instead,
 the `GuildDownloadCompleted` event should be used. The `Ready` event is only meant to signal that the client has
 finished the initial handshake with the gateway and is prepared to begin sending payloads.

<!-- LINKS -->
[0]:  https://en.wikipedia.org/wiki/Deadlock

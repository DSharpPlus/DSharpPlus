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
private async Task MainAsync()
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
private async Task MainAsync()
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

# Avoiding Deadlocks
Despite the fact that your event handlers are executed asynchronously, they are also executed one at a time on the
gateway thread for consistency. This means that each handler must complete its execution before others can be
dispatched. 

Because of this, executing code in your event handlers that runs for an extended period of time may inadvertently create
brief unresponsiveness or, even worse, cause a [deadlock][0]. To prevent such issues, any event handler that has the
potential to take more than 2 seconds to execute should have its logic offloaded to a `Task.Run`.
```cs
discord.MessageCreated += (s, e) =>
{
    _ = Task.Run(async () =>
    {
        // Pretend this takes many, many seconds to execute.
        var response = await QuerySlowWebServiceAsync(e.Message.Content);

        if (response.Status == HttpStatusCode.OK)
		{
			await e.Guild?.BanMemberAsync((DiscordMember)e.Author);
        }
    });

	return Task.CompletedTask;
};
```

Doing this will allow the handler to complete its execution quicker, which will in turn allow other handlers to be
executed and prevent the gateway thread from being blocked.

<!-- LINKS -->
[0]:  https://en.wikipedia.org/wiki/Deadlock

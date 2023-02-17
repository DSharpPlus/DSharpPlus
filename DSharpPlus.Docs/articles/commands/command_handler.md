---
uid: articles.commands.command_handler
title: Custom Command Handler
---

## Custom Command Handler
> [!IMPORTANT]
> Writing your own handler logic should only be done if *you know what you're doing*. You will be responsible for
> command execution and preventing deadlocks.
 
### Disable Default Handler
To begin, we'll need to disable the default command handler provided by CommandsNext. This is done by setting the
@DSharpPlus.CommandsNext.CommandsNextConfiguration.UseDefaultCommandHandler configuration property to `false`.
```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
{
    UseDefaultCommandHandler = false
});
```

### Create Event Handler
We'll then write a new handler for the @DSharpPlus.DiscordClient.MessageCreated event fired from 
@DSharpPlus.DiscordClient.
```cs
discord.MessageCreated += CommandHandler;

// ...

private Task CommandHandler(DiscordClient client, MessageCreateEventArgs e)
{
    // See below ...
}
```

This event handler will be our command handler, and you'll need to write the logic for it.

### Handle Commands
Start by parsing the message content for a prefix and command string
```cs
var cnext = client.GetCommandsNext();
var msg = e.Message;

// Check if message has valid prefix.
var cmdStart = msg.GetStringPrefixLength("!");
if (cmdStart == -1) return;

// Retrieve prefix.
var prefix = msg.Content.Substring(0, cmdStart);

// Retrieve full command string.
var cmdString = msg.Content.Substring(cmdStart);
```

Then provide the command string to @DSharpPlus.CommandsNext.CommandsNextExtension.FindCommand*.
```cs
var command = cnext.FindCommand(cmdString, out var args);
```

Create a command context using our message and prefix, along with the command and its arguments
```cs
var ctx = cnext.CreateContext(msg, prefix, command, args);
```

And pass the context to @DSharpPlus.CommandsNext.CommandsNextExtension.ExecuteCommandAsync* to execute the command.
```cs
_ = Task.Run(async () => await cnext.ExecuteCommandAsync(ctx));
// Wrapped in Task.Run() to prevent deadlocks.
```

### Finished Product
Altogether, your implementation should function similarly to the following:
```cs
private Task CommandHandler(DiscordClient client, MessageCreateEventArgs e)
{
    var cnext = client.GetCommandsNext();
    var msg = e.Message;

    var cmdStart = msg.GetStringPrefixLength("!");
    if (cmdStart == -1) return Task.CompletedTask;

    var prefix = msg.Content.Substring(0, cmdStart);
    var cmdString = msg.Content.Substring(cmdStart);

    var command = cnext.FindCommand(cmdString, out var args);
    if (command == null) return Task.CompletedTask;

    var ctx = cnext.CreateContext(msg, prefix, command, args);
    Task.Run(async () => await cnext.ExecuteCommandAsync(ctx));
	
    return Task.CompletedTask;
}
```

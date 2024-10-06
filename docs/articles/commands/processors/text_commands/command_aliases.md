---
uid: articles.commands.command_processors.text_commands.command_aliases
title: Command Aliases
---

# Command Aliases
To add an alias to a command, simply add the `TextAlias` attribute to the method that defines the command. It should be noted that the aliases are applied *only* to text commands.

```cs
public static class PingCommand
{
    [Command("ping")]
    [TextAlias("pong")]
    public static async ValueTask ExecuteAsync(CommandContext context) => await context .RespondAsync("Pong!");
}
```

In this example, the `PingCommand` command can be invoked by either `!ping` or `!pong`.
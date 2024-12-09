---
uid: articles.commands.command_processors.introduction
title: Command Processors Introduction
---

# Command Processors
Each processor has features specific to it. For example, the `SlashCommand` processor has support for choice providers
and auto-complete, while the `TextCommand` processor has support for command aliases. Each section will be named after
their own processor, explaining which features are available and how to use them.

## Filter allowed processors
The extension allows you to configure what processors can execute a specific command. This is useful if you want to have commands that are only available for text or slash commands.

There are two ways to accomplish this filtering:

### Filter with the `AllowedProcessors` attribute

Apply the `AllowedProcessors` attribute to your command, specifying the allowed processors:

```csharp
[Command("debug")]
public class InfoCommand
{
    [Command("textCommand"), AllowedProcessors<TextCommandProcessor>()]
    public static async ValueTask TextOnlyAsync(CommandContext context) =>
        await context.RespondAsync("This is a text command");

    [Command("slashCommand"), AllowedProcessors<SlashCommandProcessor>()]
    public static async ValueTask SlashOnlyAsync(CommandContext context) =>
        await context.RespondAsync("This is a slash command");
}
```

The attribute can only be applied to the top-level command, and will be inherited by all subcommands.

### Filter with concrete `CommandContext` types

If you use a specific command context instead of the default `CommandContext` the command is only registered
to processors which context is assignable to that specific type

```csharp
[Command("debug")]
public class InfoCommand
{
    [Command("textCommand")]
    public static async ValueTask TextOnlyAsync(TextCommandContext context) =>
        await context.RespondAsync("This is a text command");

    [Command("slashCommand")]
    public static async ValueTask SlashOnlyAsync(SlashCommandContext context) =>
        await context.RespondAsync("This is a slash command");
}
```
---
uid: articles.commands.command_processors.introduction
title: Command Processors Introduction
---

# Command Processors
Each processor has features specific to it. For example, the `SlashCommand` processor has support for choice providers
and auto-complete, while the `TextCommand` processor has support for command aliases. Each section will be named after
their own processor, explaining which features are available and how to use them.

## Filter allowed processors
The extension allows you to configure what processors are allowed to register a certain command.

There are two ways to accomplish this filtering:

### Filter with attribute

Apply the `AllowedProcessor` attribute to your command and the command with all subcommands is only allowed on the given
processors:

```csharp
[Command("debug")]
public class InfoCommand
{
    // only usable throug a text message
    [Command("textCommand"), AllowedProcessors(typeof(TextCommandProcessor))]
    public static ValueTask TextOnlyAsync(CommandContext context) => default;

    // only usable through a slashcommand
    [Command("slashCommand"), AllowedProcessors(typeof(SlashCommandProcessor))]
    public static ValueTask TextOnlyAsync2(TextCommandContext context) => default;

```

### Filter with concrete commandcontext types

If you use a specific command context instead of the default `CommandContext` the command is only registered
to processors which context is assignable to that specific type

```csharp
[Command("debug")]
public class InfoCommand
{
    // only usable throug a text message
    [Command("textCommand")]
    public static ValueTask TextOnlyAsync(TextCommandContext context) => default;

    // only usable through a slashcommand
    [Command("slashCommand")]
    public static ValueTask TextOnlyAsync2(SlashCommandContext context) => default;

```
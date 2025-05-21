---
uid: articles.analyzers.commands
title: DSharpPlus.Commands Analyzer Rules
---

# DSharpPlus.Commands Rules

This page documents the analyzer rules defined for APIs defined in DSharpPlus.Commands and their associated usage
patterns:

- [DSP1001](#usage-error-dsp1001)
- [DSP1002](#usage-warning-dsp1002)
- [DSP1003](#usage-error-dsp1003)

## Usage Error DSP1001

A slash command explicitly registered to a guild should not be installable to users.

Slash commands registered to a guild are restricted to that guild and cannot be referenced outside of it, but a
registered installation type requires it to be usable outside the guild. Either remove the specified installation type
or remove the `RegisterToGuilds` attribute.

The following sample will generate DSP1001:

```csharp
public class PingCommand 
{
    [Command("ping")]
    [RegisterToGuilds(379378609942560770)]
    [InteractionInstallType(DiscordApplicationIntegrationType.UserInstall)]
    public static async ValueTask ExecuteAsync(CommandContext ctx) 
    {
        await ctx.RespondAsync("Pong!");
    }
}
```

## Usage Warning DSP1002

Do not explicitly register nested classes of elsewhere-registered classes to DSharpPlus.Commands.

Do not register nested classes. If their containing class gets registered as well, the commands inside the nested class
get registered twice.

The following sample will generate DSP1002:

```csharp
public class Registerator
{
    public static void Register(CommandsExtension extension) 
    {
        extension.AddCommands([typeof(ACommands.BCommands), typeof(ACommands)]);
    }
}

[Command("a")]
public class ACommands
{
    [Command("b")]
    public class BCommands 
    {
        [Command("c")]
        public static async ValueTask CAsync(CommandContext context) 
        {
            await context.RespondAsync("C");
        }
    } 
}
```

## Usage Error DSP1003

A command taking a specific context type should not restrict itself to other processors.

Specifying a command context type acts as a form of filtering where the command will only be executable by processors
capable of creating the demanded context. In a similar vein, `AllowedProcessorAttribute` acts as a form of filtering
where the command will only be executable by one of the listed processors. Filtering to two sets of processors that are
not compatible with one another will render your command partially or wholly unusable.

The following sample will generate DSP1003:

```csharp
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.SlashCommands;

public class Test
{
    [AllowedProcessors<SlashCommandProcessor>]
    public async Task Tester(TextCommandContext context)
    {
        await context.RespondAsync("Tester!");
    }
}
```
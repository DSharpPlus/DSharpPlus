---
uid: articles.analyzers.commands
title: DSharpPlus.Analyzer commands rules
---

# DSharplus core library

All the rules related to the `DSharpPlus.Commands` library.  
Any `DSharpPlus.Commands` rule will follow the format `DSP1xxx`

## Usage error DSP1001

A slash command explicitly registered to a guild should not specify DMs or user apps as installable context.

The analyzer found a command that tries to register to a guild but uses installable contexts that do not support
guilds.
Instead, either remove the specified install type or remove the `RegisterToGuilds` attribute.

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

## Usage warning DSP1002

Do not explicitly register nested classes of elsewhere-registered classes to DSharpPlus.Commands

The analyzer detected a nested class that is trying to be registered manually.
Instead, only register the parent class.

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

## Usage error DSP1003
A command taking a specific context type should not be registered as allowing processors whose contex type it doesn't support.

The analyzer detected a command registering itself to a processor and specifying a context type it doesn't support.
Instead, change the context type to use something the processor does support.

The following sample will generate DSP1003:

```csharp
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.SlashCommands;

public class Test
{
    [AllowedProcessors<SlashCommandProcessor>()]
    public async Task Tester(TextCommandContext context)
    {
        await context.RespondAsync("Tester!");
    }
}
```
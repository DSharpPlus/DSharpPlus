---
uid: articles.analyzers.rules
title: DSharpPlus.Analyzer rules
---

# DSharpPlus.Analyzer errors

Here we will list out a detailed description for each error and when they will appear.

# DSharplus core library

All the rules related to the core `DSharpPlus` library.  
Any core rule will follow the format `DSP0xxx`

## Usage warning DSP0005

`Permission.HasPermission` should always be prefered over bitwise operations.

The analyzer detected that bitwise operations are used instead of the prefered `Permission.HasPermission` method.

The following sample will generate DSP0005:

```csharp
public class PermissionExample 
{
    public static bool HasManageGuild(DiscordPermissions permissions)
    {
        return (permissions & DiscordPermissions.ManageGuild) != 0;
    }
}
```

## Design warning DSP0006

Use 'ModifyAsync' instead of 'AddOverwriteAsnyc'.

The analyzer detected that multiple 'AddOverwriteAsnyc' calls are happening in the same method body.
This will/can cause multiple request to happen to the same channel.  
Instead prefer using 'ModifyAsync' to minimise this to a single request.

The following sample will generate DSP0006:

```csharp
public class PermissionOverwriteExample 
{
    public static async Task UpdatePermissionsAsync(DiscordChannel channel, List<DiscurdMember> members, DiscordPermissions newAllowed) 
    {
        foreach (DiscordUser member in members) 
        {
            await channel.AddOverwriteAsync(member, newAllowed);
        }
    }
}
```

## Design info DSP0007

Use a list request instead of fetching single entities inside of a loop.

The analyzer detected that there is single entities being fetched in a loop.  
Instead if there is only one entity being requested put it outside the loop.  
If there is multiple entities, prefer pagination related methods.

The folllowing sample will generate DSP0007:

```csharp
// No good example yet
```

# DSharpPlus.Commands

All the rules related to the `DSharpPlus.Commands` library.  
Any `DSharpPlus.Commands` rule will follow the format `DSP1xxx`.

## Usage error DSP1001

A slash command explicitly registered to a guild should not specify DMs or user apps as installable context.

The analyzer detected a command that tries to register to a guild but uses installable contexts that does not support
guild usage.
Instead either remove the specified install type or remove the guild registration specification.

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
Instead only register the parent class

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
A command taking a specific context type should not be registerd as allowing processors whose contex type it doesn't support.

The analyzer detected a command registering itself to a processor and specifying a context type it doesn't support.  
Instead change the context type to use something the processor supports

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

# DSharpPlus.Interactivity

All the rules related to the `DSharpPlus.Interactivity` library.  
Any `DSharpPlus.Interactivity` rule will follow the pattern `DSP2xxx`

There seems to be no rules for this section yet... come back later!

# DSharpPlus.Voice

All the rules related to the `DSharpPlus.Voice` library.  
Any `DSharpPlus.Voice` rule will follow the pattern `DSP3xxx`

There seems to be no rules for this section yet... come back later!
---
uid: articles.analyzers.errors
title: DSharpPlus.Analyzer errors
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
Use @DiscordChannel.ModifyAsync instead of @DiscordChannel.AddOverwriteAsnyc.

The analyzer detected that multiple @DiscordChannel.AddOverwriteAsnyc calls are happening in the same method body. 
This will/can cause multiple request to happen to the same channel.  
Instead prefer using @DiscordChannel.ModifyAsync to minimise this to a single request.

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

The folllowing sample will generate DSP0007
```csharp
// No good example yet
```

# DSharpPlus.Commands
All the rules related to the `DSharpPlus.Commands` library.  
Any `DSharpPlus.Commands` rule will follow the format `DSP1xxx`.

## Usage error DSP1001

## Usage warning DSP1002

## Usage error DSP1003

# DSharpPlus.Interactivity
All the rules related to the `DSharpPlus.Interactivity` library.  
Any `DSharpPlus.Interactivity` rule will follow the pattern `DSP2xxx`

There seems to be no rules for this section yet... come back later!

# DSharpPlus.Voice
All the rules related to the `DSharpPlus.Voice` library.  
Any `DSharpPlus.Voice` rule will follow the pattern `DSP3xxx`

There seems to be no rules for this section yet... come back later!
---
uid: articles.analyzers.core
title: DSharpPlus.Analyzer core rules
---

# DSharplus core library

All the rules related to the core `DSharpPlus` library.  
Any core rule will follow the format `DSP0xxx`

## Usage warning DSP0005

`DiscordPermissions.HasPermission` should always be preferred over bitwise operations.

The analyzer detected that bitwise operations are used instead of the preferred `DiscordPermissions.HasPermission` method.

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

Use `ModifyAsync` instead of `AddOverwriteAsync`.

The analyzer detected that multiple `AddOverwriteAsync` calls are happening in the same method body.
This will/can cause multiple requests to happen on the same channel.
Instead, prefer using `ModifyAsync` to minimize this to a single request.

The following sample will generate DSP0006:

```csharp
public class PermissionOverwriting
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

Use a pagination method instead of fetching single entities inside of a loop.

The analyzer detected that there are single entities being fetched in a loop.  
Instead, if there is only one entity being requested, put it outside the loop.
If there are multiple entities, prefer pagination-related methods.

The following sample will generate DSP0007:

```csharp
public class GetSpecificGuilds() 
{
    public static async Task<List<DiscordGuild>> GetTheseGuilds(DiscordClient client, List<ulong> ids) 
    {
        List<DiscordGuild> guilds = new(ids.Count);
        foreach (ulong id in ids) 
        {
            DiscordGuild guild = await client.GetGuildAsync(id);
            guilds.Add(guild);
        }
        
        return guilds;
    }
}
```

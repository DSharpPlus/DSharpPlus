---
uid: articles.analyzers.core
title: DSharpPlus Core Library Analyzer Rules
---

This page documents the analyzer rules defined for APIs defined in the DSharpPlus core library, `DSharpPlus.dll`, and their associated usage patterns:
- [DSP0005](#usage-warning-dsp0005)
- [DSP0006](#design-warning-dsp0006)
- [DSP0007](#design-info-dsp0007)

### Usage warning DSP0005

`DiscordPermissions.HasPermission` should always be preferred over bitwise operations.

Bitwise operations risk missing Administrator permissions, making a direct bitwise check unreliable. Use `HasPermission`, `HasAnyPermission` or `HasAllPermissions` instead as appropriate.

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

### Design warning DSP0006

Use `ModifyAsync` instead of multiple calls to `AddOverwriteAsync`.

Multiple calls to `AddOverwriteAsync` on the same channel can cause multiple requests to happen on the same channel. Instead, prefer using `ModifyAsync` with the aggregated overwrites to minimize this to a single request.

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

### Design info DSP0007

Use a bulk-fetching method instead of fetching single entities inside of a loop.

Fetching single entities individually incurs one request each. If there is only one entity being requested, put it outside the loop. If there are multiple entities, prefer bulk-fetching methods.

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

---
uid: articles.analyzers.core
title: DSharpPlus Core Library Analyzer Rules
---

# DSharpPlus Core Rules

This page documents the analyzer rules defined for APIs defined in the DSharpPlus core library, `DSharpPlus.dll`, and
their associated usage patterns:

- [DSP0006](#usage-warning-dsp0006)
- [DSP0007](#design-warning-dsp0007)
- [DSP0008](#design-info-dsp0008)
- [DSP0009](#usage-error-dsp0009)
- [DSP0010](#usage-info-dsp0010)

### Usage Warning DSP0006

`DiscordPermissions.HasPermission` should always be preferred over bitwise operations.

Bitwise operations risk missing Administrator permissions, making a direct bitwise check unreliable. Use
`HasPermission`, `HasAnyPermission` or `HasAllPermissions` instead as appropriate.

The following sample will generate DSP0006:

```csharp
public class PermissionExample 
{
    public static bool HasManageGuild(DiscordPermissions permissions)
    {
        return (permissions & DiscordPermissions.ManageGuild) != 0;
    }
}
```

### Design Warning DSP0007

Use `ModifyAsync` instead of multiple calls to `AddOverwriteAsync`.

Multiple calls to `AddOverwriteAsync` on the same channel can cause multiple requests to happen on the same channel.
Instead, prefer using `ModifyAsync` with the aggregated overwrites to minimize this to a single request.

The following sample will generate DSP0007:

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

### Design Info DSP0008

Use a bulk-fetching method instead of fetching single entities inside of a loop.

Fetching single entities individually incurs one request each. If there is only one entity being requested, put it
outside the loop. If there are multiple entities, prefer bulk-fetching methods.

The following sample will generate DSP0008:

```csharp
public class GetSpecificGuilds
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

### Usage Error DSP0009

Use `DiscordPermissions` and its operators instead of doing operations on `DiscordPermission`.

@DSharpPlus.Entities.DiscordPermission is only an enum containing each permission flag. It does not take care of multiple permissions.
@DSharpPlus.Entities.DiscordPermissions represents multiple permissions and has utilities for managing this, use it over @DSharpPlus.Entities.DiscordPermission when representing multiple permissions. 
Performing any math on an instance of `DiscordPermission` is incorrect behaviour.

The following sample will generate DSP0009:

```csharp
public class PermissionUtility
{
    public static DiscordPermission AddAdmin(DiscordPermission permission) 
    {
        return permission | DiscordPermission.Administrator;
    }
}
```

### Usage Info DSP0010

Use explicit methods on `DiscordPermissions` rather than bitwise operators.

DiscordPermissions provides explicit ways to accomplish common operations that should be preferred for clarity and performance:

| Bitwise Operation | Explicit Operation |
| - | - |
| <code>permissions &#124; other</code> | `permissions + other` |
| `permissions & (~other)` | `permissions - other` |
| `permissions ^ other` | `permissions.Toggle(other)` |

The following sample will generate DSP0010:

```csharp
public class PermissionUtility
{
    public static DiscordPermissions Problem(DiscordPermissions input, DiscordPermissions mask)
    {
        return input & (~mask);
    }
}
```

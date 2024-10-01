---
uid: articles.commands.multi_argument_parameters
title: Multi-Argument Parameters
---

# Multi-Argument Parameters

When creating a command, you may want to have a parameter that can accept multiple arguments. This is useful for commands that require a list of items, such as a list of users or a list of numbers. This was previously supported in the `CommandsNext` and `SlashCommands` extensions through the `params` keyword.

```csharp
[Command("echo")]
[Description("Repeats a message.")]
public async ValueTask ExecuteAsync(CommandContext context, params string[] args) =>
    await context.RespondAsync(string.Join(' ', args));
```

Which could be used like this:

```
!echo hello world 1 2 3
```

This behavior is still supported, but what if you wanted to have multiple lists? For example, a command that takes in a list of users and a list of numbers. This is where multi-argument parameters come in.

```csharp
[Command("assign")]
[Description("Assigns multiple roles to multiple users.")]
public async ValueTask ExecuteAsync(
    CommandContext context,
    [MultiArgument(1, 5)] IReadOnlyList<DiscordRole> roles,
    [MultiArgument(1)] IReadOnlyList<DiscordMember> members
)
{
    // We're making a lot of API calls here, so let the
    // user know that we've received the command and we
    // are doing work in the background.
    await context.DeferResponseAsync();
    foreach (DiscordMember member in members)
    {
        List<DiscordRole> memberRoles = new(member.Roles);
        memberRoles.AddRange(roles);
        memberRoles = memberRoles.Distinct().ToList();
        await member.ModifyAsync(member => member.Roles = memberRoles);
    }

    await context.RespondAsync($"Assigned {roles.Count} roles to {members.Count} members.");
}
```

In this example, the `roles` parameter will accept between 1 and 5 roles, and the `members` parameter will accept at least 1 member. This allows you to have multiple lists of arguments in a single command.
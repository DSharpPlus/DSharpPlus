---
uid: beyond_basics_builder_rolebuilder
title: Role Builder
---

## Background
Before the Role builder was put into place, we had many large methods for creating a role and many methods for modifing.  This
was becoming a major code smell and it was hard to maintain and add more params onto it. Now we support either sending a prebuilt 
builder OR an action of a builder.  

## Using the Builder
The API Documentation for the Role builder can be found at @DSharpPlus.Entities.DiscordRoleCreateBuilder and @DSharpPlus.Entities.DiscordRoleModifyBuilder but here we'll go over some of the concepts of using the
role builder:

### Creating a Role
When Creating a role, you can create it by pre-building the builder then calling CreateAsync 
```cs
await new DiscordRoleCreateBuilder()
    .WithName("Role A")
    .WithColor(DiscordColor.Blurple)
    .WithHoist(true)
    .WithMentionable(true)
    .WithPermissions(Permissions.AddReactions)
    .CreateAsync(ctx.Guild);
```
OR u can create the role by using an action

```cs
await ctx.Guild.CreateRoleAsync(x =>
{
    x.WithName("Role B")
    .WithColor(DiscordColor.Blurple)
    .WithHoist(true)
    .WithMentionable(true)
    .WithPermissions(Permissions.AddReactions);
});
```

### Modifing a Role

When modifing a role, you can create it by pre-building the builder then calling ModifyAsync 
```cs 
await new DiscordRoleModifyBuilder()
    .WithName("Role C")
    .ModifyAsync(role);
```

OR u can modify the role by using an action

```cs
await role.ModifyAsync(x => {
    x.WithName("Role D");
});
```
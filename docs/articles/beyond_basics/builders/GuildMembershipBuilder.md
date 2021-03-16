---
uid: beyond_basics_builder_guildmembershipbuilder
title: Guild Membership Screening Builder
---

## Background
Before the membership screening builder was put into place, it would take many params.  This was becoming a major code smell and it was hard to maintain and add more params onto it. Now we support either sending a prebuilt 
builder OR an action of a builder.  

## Using the Builder
The API Documentation for the Guild Membership Screening builder can be found at @DSharpPlus.Entities.GuildMembershipModifyBuilder but here we'll go over some of the concepts of using the
role builder:

### Modifing a Member

When modifing the membership screening, you can create it by pre-building the builder then calling ModifyAsync 
```cs 
await new GuildMembershipModifyBuilder()
    .WithEnabled(true)
    .WithDescription("An awesome test description")
    .WithField(new GuildMembershipScreeningField(MembershipScreeningFieldType.Terms, "This is a Test", new string[] { "I repeat this is a test" }, true))
    .ModifyAsync(ctx.Guild);
```

OR u can modify the membership screening by using an action

```cs
await ctx.Guild.ModifyMembershipScreeningFormAsync(x => {
    x.WithEnabled(false);
});
```
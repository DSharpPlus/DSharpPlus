---
uid: beyond_basics_builder_memberbuilder
title: Member Builder
---

## Background
Before the Member builder was put into place, many methods for modifing.  This was becoming a major code smell and it was hard to maintain and add more params onto it. Now we support either sending a prebuilt 
builder OR an action of a builder.  

## Using the Builder
The API Documentation for the Role builder can be found at @DSharpPlus.Entities.DiscordMemberModifyBuilder but here we'll go over some of the concepts of using the
role builder:

### Modifing a Member

When modifing a member, you can create it by pre-building the builder then calling ModifyAsync 
```cs 
await new DiscordMemberModifyBuilder()
    .WithNickname("DummyTester")
    .ModifyAysnc(member);
```

OR u can modify the member by using an action

```cs
await member.ModifyAsync(m => {
    m.WithVoiceChannel(vcahnnel.Value)
    .WithDeafned(true)
    .WithMute(true);
});
```
---
uid: beyond_basics_builder_channelbuilder
title: Channel Builder
---

## Background
Before the channel builder was put into place, we had many large methods for creating a channel and many methods for modifying.  This
was becoming a major code smell and it was hard to maintain and add more params onto it. Now we support either sending a prebuilt 
builder OR an action of a builder.  

## Using the Builder
The API Documentation for the channel builder can be found at @DSharpPlus.Entities.ChannelCreateBuilder and @DSharpPlus.Entities.ChannelModifyBuilder but here we'll go over some of the concepts of using the
channel builder:

### Creating a Channel
When Creating a channel, you can create it by pre-building the builder then calling CreateAsync 
```cs
var builder = await new ChannelCreateBuilder()
    .WithType(ChannelType.Category)
    .WithName("Awesome Category")
    .CreateAsync(ctx.Guild);
```
OR u can create the channel by using an action

```cs
await ctx.Guild.CreateChannelAsync(x =>
{
    x.WithName("Awesome Channel")
    .WithType(ChannelType.Text)
    .WithRateLimit(25)
    .WithTopic("This is an awesome topic");
});
```

### Modifing a Channel

When modifing a channel, you can create it by pre-building the builder then calling ModifyAsync 
```cs 
await new ChannelModifyBuilder()
    .WithParentId(category.Id)
    .ModifyAsync(ctx.Channel);
```

OR u can modify the channel by using an action

```cs
await channel.ModifyAsync(x =>
{
    x.WithParentId(category.Id);
});
```
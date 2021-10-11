---
uid: articles.interactivity
title: Interactivity Introduction
---

# Introduction to Interactivity
Interactivity will enable you to write commands which the user can interact with through reactions and messages.
The goal of this article is to introduce you to the general flow of this extension.

Make sure to install the `DSharpPlus.Interactivity` package from NuGet before continuing.

![Interactivity NuGet][0]

## Enabling Interactivity
Interactivity can be registered using the
@DSharpPlus.Interactivity.Extensions.ClientExtensions.UseInteractivity(DSharpPlus.DiscordClient,DSharpPlus.Interactivity.InteractivityConfiguration)
extension method. Optionally, you can also provide an instance of @DSharpPlus.Interactivity.InteractivityConfiguration
to modify default behaviors.

```cs
var discord = new DiscordClient();

discord.UseInteractivity(new InteractivityConfiguration() 
{ 
    PollBehaviour = PollBehaviour.KeepEmojis,
    Timeout = TimeSpan.FromSeconds(30)
});
```

## Using Interactivity
There are two ways available to use interactivity: 

* Extension methods available for @DSharpPlus.Entities.DiscordChannel and @DSharpPlus.Entities.DiscordMessage.
* [Instance methods][1] available from @DSharpPlus.Interactivity.InteractivityExtension.

We'll have a quick look at a few common interactivity methods along with an example of use for each.

The first (and arguably most useful) extension method is
@DSharpPlus.Interactivity.InteractivityExtension.SendPaginatedMessageAsync* for @DSharpPlus.Entities.DiscordChannel

This method displays a collection of *'pages'* which are selected one-at-a-time by the user through reaction buttons.
Each button click will move the page view in one direction or the other until the timeout is reached.

You'll need to create a collection of pages before you can invoke this method. This can be done easily using the
@DSharpPlus.Interactivity.InteractivityExtension.GeneratePagesInEmbed* and
@DSharpPlus.Interactivity.InteractivityExtension.GeneratePagesInContent* instance methods from
@DSharpPlus.Interactivity.InteractivityExtension.
Alternatively, for pre-generated content, you can create and add individual instances of @DSharpPlus.Interactivity.Page
to a collection.

This example will use the @DSharpPlus.Interactivity.InteractivityExtension.GeneratePagesInEmbed* method to generate the
pages.
```cs
public async Task PaginationCommand(CommandContext ctx)
{
    var reallyLongString = "Lorem ipsum dolor sit amet, consectetur adipiscing ..."

    var interactivity = ctx.Client.GetInteractivity();
    var pages = interactivity.GeneratePagesInEmbed(reallyLongString);

    await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
}
```

![Pagination Pages][2]

Next we'll look at the @DSharpPlus.Interactivity.Extensions.MessageExtensions.WaitForReactionAsync* extension method for
@DSharpPlus.Entities.DiscordMessage. This method waits for a reaction from a specific user and returns the emoji that
was used.

An overload of this method also enables you to wait for a *specific* reaction, as shown in the example below.
```cs
public async Task ReactionCommand(CommandContext ctx, DiscordMember member)
{
    var emoji = DiscordEmoji.FromName(ctx.Client, ":ok_hand:");
    var message = await ctx.RespondAsync($"{member.Mention}, react with {emoji}.");

    var result = await message.WaitForReactionAsync(member, emoji);

    if (!result.TimedOut) await ctx.RespondAsync("Thank you!");
}
```

![Thank You!][3]

Another reaction extension method for @DSharpPlus.Entities.DiscordMessage is
@DSharpPlus.Interactivity.InteractivityExtension.CollectReactionsAsync* As the name implies, this method collects all
reactions on a message until the timeout is reached.
```cs
public async Task CollectionCommand(CommandContext ctx)
{
    var message = await ctx.RespondAsync("React here!");
    var reactions = await message.CollectReactionsAsync();

    var strBuilder = new StringBuilder();
    foreach (var reaction in reactions)
    {
        strBuilder.AppendLine($"{reaction.Emoji}: {reaction.Total}");
    }

    await ctx.RespondAsync(strBuilder.ToString());
}
```

![Reaction Count][4]

The final one we'll take a look at is the @DSharpPlus.Interactivity.Extensions.ChannelExtensions.GetNextMessageAsync*
extension method for @DSharpPlus.Entities.DiscordMessage.

This method will return the next message sent from the author of the original message. Our example here will use its
alternate overload which accepts an additional predicate.
```cs
public async Task ActionCommand(CommandContext ctx)
{
    await ctx.RespondAsync("Respond with *confirm* to continue.");
    var result = await ctx.Message.GetNextMessageAsync(m =>
    {
        return m.Content.ToLower() == "confirm";
    });

    if (!result.TimedOut) await ctx.RespondAsync("Action confirmed.");
}
```

![Confirmed][5]

<!-- LINKS -->
[0]:  /images/interactivity_01.png
[1]:  xref:DSharpPlus.Interactivity.InteractivityExtension#methods
[2]:  /images/interactivity_02.png
[3]:  /images/interactivity_03.png
[4]:  /images/interactivity_04.png
[5]:  /images/interactivity_05.png
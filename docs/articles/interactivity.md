---
uid: interactivity
title: Interactivity Introduction
---

# Introduction to Interactivity
Interactivity will enable you to write commands which the user can interact with through reactions and messages.
The goal of this article is to introduce you to the general flow of this extension.

Make sure to install the `DSharpPlus.Interactivity` package from NuGet before continuing.

![Interactivity NuGet](/images/interactivity_01.png)

## Enabling Interactivity
Interactivity can be registered using the `DiscordClient#UseInteractivity()` extension method.<br/>
Optionally, you can also provide an instance of `InteractivityConfiguration` to modify default behaviors.

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

* Extension methods available for `DiscordChannel` and `DiscordMessage`.
* [Instance methods](xref:DSharpPlus.Interactivity.InteractivityExtension#methods) available from `InteractivityExtension`.

We'll have a quick look at a few common interactivity methods along with an example of use for each.

<br/>
The first (and arguably most useful) extension method is `SendPaginatedMessageAsync` for `DiscordChannel`.

This method displays a collection of *'pages'* which are selected one-at-a-time by the user through reaction buttons.
Each button click will move the page view in one direction or the other until the timeout is reached.

You'll need to create a collection of pages before you can invoke this method. 
This can be done easily using the `GeneratePagesInEmbed` and `GeneratePagesInContent` instance methods from `InteractivityExtension`.<br/>
Alternatively, for pre-generated content, you can create and add individual instances of `Page` to a collection.

This example will use the `GeneratePagesInEmbed` method to generate the pages.
```cs
public async Task PaginationCommand(CommandContext ctx)
{
    var reallyLongString = "Lorem ipsum dolor sit amet, consectetur adipiscing ..."

    var interactivity = ctx.Client.GetInteractivity();
    var pages = interactivity.GeneratePagesInEmbed(reallyLongString);

    await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages);
}
```

![Pagination Pages](/images/interactivity_02.png)

<br/>
Next we'll look at the `WaitForReactionAsync` extension method for `DiscordMessage`.<br/>
This method waits for a reaction from a specific user and returns the emoji that was used.

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

![Thank You!](/images/interactivity_03.png)

<br/>
Another reaction extension method for `DiscordMessage` is `CollectReactionsAsync`.<br/>
As the name implies, this method collects all reactions on a message until the timeout is reached.
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

![Reaction Count](/images/interactivity_04.png)

<br/>
The final one we'll take a look at is the `GetNextMessageAsync` extension method for `DiscordMessage`.<br/>

This method will return the next message sent from the author of the original message.<br/>
Our example here will use its alternate overload which accepts an additional predicate.
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

![Confirmed](/images/interactivity_05.png)
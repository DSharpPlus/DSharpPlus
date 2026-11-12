---
uid: articles.interactivity.intro
title: Introduction to Interactivity
---

DSharpPlus.Interactivity is a toolkit to make creating interactive flows with the user easier. It provides a number of basic building blocks to wait for reactions, buttons, select menus, modals et cetera.

To use interactivity, we first need to install the package and then register interactivity with our client:

# [Main Method](#tab/main-method)

~~~cs
DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(...);

builder.UseInteractivity();

// to configure:
builder.ConfigureInteractivity(options => 
{
    options.Timeout = TimeSpan.FromSeconds(30);
});
~~~

# [Service Collection](#tab/service-collection)

~~~cs
services.AddDiscordClient(...);

services.AddInteractivityExtension();

// to configure:
services.Configure<InteractivityOptions>(options =>
{
    options.Timeout = TimeSpan.FromSeconds(30);
})
~~~

---

Having thus enabled interactivity for our bot, we can start using it. Interactivity provides a number of extension methods on messages, channels and interactions (the following sample is non-exhaustive):

- `DiscordMessage.WaitForButton/SelectMenu/ReactionAsync` provide ways to wait for a button press, selection or reaction to a message,
- `DiscordMessage.CollectReactionsAsync` collects reactions added to a message over the specified timespan,
- `DiscordMessage.WaitForReplyAsync` waits for a reply to the message to be sent,
- `DiscordChannel.WaitForMessageAsync` waits for the next message in the channel,
- `DiscordInteraction.SendAndWaitForModalAsync` sends a modal in response to an interaction and waits for the user to submit their response.

> [!NOTE]
> The default timeout is specified in the interactivity configuration, and all of these methods can take a timeout override. We recommend keeping interactivity timeouts within reason: it is an utility designed for small-scope, local utilities, not for processes that span hours. Consider building your system using [events](xref:articles.beyond_basics.events) instead if it runs over long periods of time.

These interactivity methods return a value of type `Result<*EventArgs>`. A result is a value that can represent either failure, with an associated error, or success, with the associated return value. In the case of interactivity, a `TimeoutError` will be returned in place of the return value if the configured timeout elapsed before the event being waited for took place, and Interactivity provides `IsTimedOut()` as a helper to check for this.

All of these methods also provide overloads for restricting which users may interact with the bot or specifying arbitrary conditions. To illustrate with an example:

~~~cs
public async Task ConfirmAsync(CommandContext ctx)
{
    // for purposes of illustration, this uses a v1 message, but it works just as well with v2 messages
    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
        .WithContent("Quo usque tandem abutere, Catilina, patientia nostra! quam diu etiam iste furor tuus nos eludet!")
        .AddActionRow(new DiscordButtonComponent(DiscordButtonStyle.Success, "mori", "Mori!"), new DiscordButtonComponent(DiscordButtonStyle.Danger, "vive", "Vive"));

    await ctx.RespondAsync(messageBuilder);
    DiscordMessage message = ctx.GetResponseAsync();

    // only the specified user will be respected
    Result<ComponentInteractionCreatedEventArgs> result = await message.WaitForButtonAsync(ctx.User);

    // IsSuccess tells us whether the operation, in principle, succeeded or failed, timeout is one particular failure mode
    if (!result.IsSuccess)
    {
        if (result.IsTimedOut())
        {
            await ctx.FollowupAsync("Confirmation timed out.");
        }
    //  else if (result.Error is SomeOtherError error)
    //  {
    //      // this is how we would check for other failure modes
    //  }
        else
        {
            await ctx.FollowupAsync("An unknown error has occurred");
        }

        return;
    }

    // process result.Value here

    if (result.Value.Id == "vive")
    {
        await result.Value.CreateResponseAsync("At mortem te, Catilina, duci iussu consulis iam pridem oportebat, in te conferri pestem quam tu in nos machinaris.");
    }
    else
    {
        await result.Value.CreateResponseAsync("Vive Catilinam! Immo vero etiam in senatum veni, publici consilii participa, nota et designa oculis ad caedem unum quemque nostrum.");
    }
}
~~~

#### Interactive messages

If the message itself is supposed to respond to user interaction, DSharpPlus exposes a system called [interactive messages](xref:articles.interactivity.interactive_messages). This enables highly customizable messages that are themselves an interactive user interface according to data preinitialized by the bot.

#### Other failure cases

Timing out is the most prominent failure case for an interactive operation - again, keep in mind that timeouts should not be too long so as to prevent interactive operations piling up - but there are other options:

- `MessageDeletedError`: returned by `CollectReactionsAsync` if the monitored message was deleted while the bot was instructed to collect reactions

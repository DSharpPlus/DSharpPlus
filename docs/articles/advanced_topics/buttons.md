---
uid: advanced_topics_buttons
title: Buttons & Components
---

# Button Introduction
Buttons are a feature in Discord based on the interaction framework. Buttons are appended to the bottom of a message, and come in several colors.

Before working with buttons, you will want to familarize yourself with the [Message Builder](xref:beyond_basics_messagebuilder). It, among other builders will be mentioned throughout this article, and function similarly.

With buttons, you can have up to five buttons in a row, and up to five (5) rows of buttons. 
Furthermore, buttons come in two types: regular, and link. Link buttons contain a Url field, and are always grey.

# Buttons Continued

> [!WARNING]
> Custom Ids on buttons should be unique, as this is what's sent back when a user presses a button.
>
> Link buttons do **not** have a custom id and do **not** send interactions when pressed.

Buttons consist of five parts:
- Id
- Style
- Label
- Emoji
- Disabled

The id of the button is a settable string on buttons, and is specified by the developer. Discord sends this id back in the [interaction object](https://discord.dev/interactions/slash-commands#interaction).

The style of a button is one of four colors: Blurple, Grey, Green, and Red. These are referred to as Primary, Secondary, Success, and Danger respectively.

The label of a button is optional *if* an emoji is specified. The label can be up to 80 characters in length. 
The emoji of a button is a [partial emoji object](https://discord.dev/interactions/message-components#component-object), which means that **any valid emoji is usable**, even if your bot does not have access to it's origin server.

The disabled field of a button is rather self explanatory. If this is set to true, the user will see a greyed out button which they cannot interact with. 

# Making buttons

Making buttons is relatively simple. Simply make a builder, and sprinkle some content and buttons on.


```cs
var builder = new DiscordMessageBuilder();

builder
    .WithContent("This message has buttons! Pretty neat innit?")
    .WithComponents(new DiscordComponent[] 
    {
        new DiscordButtonComponent(ButtonStyle.Primary, "1_top" "Blurple!"),
        new DiscordButtonComponent(ButtonStyle.Secondary, "2_top", "Grey!"),
        new DiscordButtonComponent(ButtonStyle.Success, "3_top", "Green!"),
        new DiscordButtonComponent(ButtonStyle.Danger, "4_top", "Red!"),
        new DiscordButtonComponent("https://some-super-cool.site", "Link!")
    })
    .WithComponents(new DiscordComponent[] 
    {
        new DiscordButtonComponent(ButtonStyle.Primary, "1_top_d" "Blurple!", true),
        new DiscordButtonComponent(ButtonStyle.Secondary, "2_top_d" "Grey!", true),
        new DiscordButtonComponent(ButtonStyle.Success, "3_top_d", "Green!", true),
        new DiscordButtonComponent(ButtonStyle.Danger, "4_top_d" "Red!", true),
        new DiscordButtonComponent("https://some-super-cool.site", "Link!", true)
    });

await builder.SendAsync(someChannel);
```
As a note, custom ids can contain spaces. For these examples, underscores will be used. 

Produces a message like such: ![Buttons](/images/advanced_topics_buttons_01.png)


# Responding to button presses

When any button is pressed, it will fire the [ComponentInteractionCreated](xref:DSharpPlus.DiscordClient#ComponentInteractionCreated) event.

In the event args, `Id` will be the id of the button you specified. There's also an `Interaction` property, which contains the interaction the event created. It's important to respond to an interaction within 3 seconds, or it will time out. Responding after this period will throw a `NotFoundException`.

With buttons, there are two new response types: `DefferedMessageUpdate` and `UpdateMessage`.

Using `DeferredMessageUpdate` lets you create followup messages via the [followup message builder](xref:DSharpPlus.Entities.DiscordFollowupMessageBuilder). The button will return to being in it's 'dormant' state, or it's 'unpushed' state, if you will. 

You have 15 minutes from that point to make followup messages. Responding to that interaction looks like such:

```cs
client.ComponentInteractionCreated += async (c, e) => 
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
    // Do things.. //
}
```

If you would like to update the message when a button is pressed, however, you'd use `UpdateMessage` instead, and pass a `DiscordInteractionResponseBuilder` with the new content you'd like.

```cs
client.ComponentInteractionCreated += async (c, e) => 
{
    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("No more buttons for you >:)"));
}
```
This will update the message, and without the infamous <sub>(edited)</sub> next to it. Nice.


# Interactivity
Along with the typical `WaitForMessageAsync` and `WaitForReactionAsync` methods provided by interactivity, there are also button implementations as well.

More information about how interactivity works can be found in [the interactivity article](xref:interactivity)

Since buttons create interactions, there are also two additional properties in the configuration:
- RepsonseBehavior
- ResponseMessage

ResponseBehavior is what interactivity will do when handling something that isn't a valid valid button, in the context of waiting for a specific button. It defaults to `Ignore`, which will cause the interaction fail.

Alternatively, setting it to `Ack` will acknowledge the button, and continue waiting. 

Respond will reply with an ephemeral message with the aforementioned response message. 

ResponseBehavior only applies to the overload accepting a string id of the button to wait for.
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
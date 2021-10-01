---
uid: articles.advanced_topics.selects
title: Select Menus
---
# Introduction
**They're here!** What's here? Select menus (aka dropdowns) of course.

Dropdowns are another [message component][0] added to the Discord API. Additionally, just like buttons, dropdowns are 
supported in all builders that take @DSharpPlus.Entities.DiscordComponent. However, dropdowns occupy an entire action
row, so you can only have up to 5! Furthermore, buttons cannot occupy the same row as a dropdown.

In this article, we will go over what dropdowns are, how to use them, and the limitations of dropdowns.

# Dropdowns overview
> [!NOTE]
> This article is under the presumption that you are familiar with buttons.
> In addition to this, just like buttons, select menu ids should be unique.

Dropdowns consist of several parts, and share some in common with buttons. They have a:
- Custom id
- Placeholder
- Disabled
- Options
- Min Values
- Max Values

So lets go over these one by one, starting with the id. The id of a dropdown should of course be unique, just like
buttons, and Discord will send this id back in the [interaction object][1].

Placeholder is also relatively relatively simple! It's hopefully self-explanatory, too. Placeholder text is the text the
user will see when no options are selected.

If you do not wish to have placeholder text, simply pass `null` as that parameter in the constructor for the dropdown.
Placeholder only supports plain-text, and up to 100 characters.

Disabled: Applies to the entire dropdown, and will grey it out if set to `true`.

Min and Max values determine how many or how few options are valid. There are few requirements, though: Min < Max, Min
\>= 0, Max > 0, Max <= 25. Simple enough, right?

"But you skipped options!", you may say, and that we have. Options are a bit more complicated, and have their own
section right below.

# Dropdown options
Dropdown options are somewhat more involved than handling buttons, but they're still relatively simple. They can have up
to **25** options, but must have at least 1. These consist of several parts:
- Label
- Value
- Default
- Description
- Emoji

Label is the label of the option. This is always required, and can be up to **100** characters long.

Value is like the custom id of the dropdown; for the most part it should be unique. This will be accessible on the
@DSharpPlus.Entities.DiscordInteractionData.Values property the interaction, and will contain all the selected options.

Individual values unfortunately cannot be disabled.

Description is text that is placed under the label on each option, and can also be up to 100 characters. This text is
also plain-text, and does not support markdown.

Default determines whether or not the option will be the default option (which overrides placeholder). If you set
multiple to default (and allow multiple to be selected), the user will see the options as pre-selected.

Emoji is the same as a button. You can pass an emoji id, a unicode emote or a DiscordEmoji object, which will
automatically use either.

> [!WARNING]
> When using DiscordComponentEmoji's string overload, you **MUST** use the unicode representation of the emoji you want.
> ex: 👋 and not \:wave\:

# Putting it all together
> [!NOTE]
> Spaces are valid in custom ids as well, but underscores will be used in this article for consistency.

Well now you know how dropdowns work, and how dropdown options work, but how do you make the darn thing???

It would look something along the lines of this:
```cs
// Create the options for the user to pick
var options = new List<DiscordSelectComponentOption>()
{
    new DiscordSelectComponentOption(
        "Label, no description",
        "label_no_desc"),

    new DiscordSelectComponentOption(
        "Label, Description",
        "label_with_desc",
        "This is a description!"),

    new DiscordSelectComponentOption(
        "Label, Description, Emoji",
        "label_with_desc_emoji",
        "This is a description!",
        emoji: new DiscordComponentEmoji(854260064906117121)),

    new DiscordSelectComponentOption(
        "Label, Description, Emoji (Default)",
        "label_with_desc_emoji_default",
        "This is a description!",
        isDefault: true,
        new DiscordComponentEmoji(854260064906117121))
};

// Make the dropdown
var dropdown = new DiscordSelectComponent("dropdown", null, options, false, 1, 2);
```
Okay, so we have a dropdown...now what? Simply pass it to any builder that constructs a response, be it a 
@DSharpPlus.Entities.DiscordMessageBuilder, @DSharpPlus.Entities.DiscordInteractionResponseBuilder, or 
@DSharpPlus.Entities.DiscordWebhookBuilder.

It'll look something like this, using the code above:
```cs
// [...] Code trunctated for brevity

var builder = new DiscordMessageBuilder()
    .WithContent("Look, it's a dropdown!")
    .AddComponents(dropdown);

await builder.SendAsync(channel); // Replace with any method of getting a channel. //
```

# Final result

Congrats! You've made a dropdown. It should look like this ![SelectImageOne][2]

When you click the dropdown, the bottom option should be pre-selected, and it will look like this. You can choose one or
two options. ![SelectImageTwo][3]

# Interactivity/Footnotes
"**Oh no, I'm getting 'This interaction failed' when selecting! What do I do?**"

Dropdowns are like buttons; when a user interacts with them, you need to respond to that interaction. 
@DSharpPlus.DiscordClient.ComponentInteractionCreated is fired from the client, just like buttons.

This applies to interactivity, too! Simply swap
@DSharpPlus.Interactivity.Extensions.MessageExtensions.WaitForButtonAsync* for
@DSharpPlus.Interactivity.Extensions.MessageExtensions.WaitForSelectAsync*, and pass a dropdown. How to go about
component-based interactivity is described [in the buttons article][4].

And that's it! Go forth and create amazing things.

<!-- LINKS -->
[0]:  https://discord.dev/interactions/message-components
[1]:  https://discord.dev/interactions/slash-commands#interaction
[2]:  /images/advanced_topics_selects_01.png
[3]:  /images/advanced_topics_selects_02.png
[4]:  xref:articles.advanced_topics.buttons

---
uid: articles.basics.messagebuilder
title: Message Builder
---

## Using the Message Builder

Discord messages, at their root, display text to readers, and this simple usecase is served by method overloads taking a simple string or `DiscordEmbedBuilder`, but messages also support a number of more complex usecases. Here's a brief summary of trivial things that require a message builder

#### Adding a File:

```cs
using fs = new FileStream("ADumbFile.txt", FileMode.Open, FileAccess.Read);

DiscordMessage msg = await new DiscordMessageBuilder()
    .WithContent("Here is a really dumb file that I am testing with.")
    .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } })
    .SendAsync(ctx.Channel);
```

#### Adding Mentions:

```cs
DiscordMessage msg = await new DiscordMessageBuilder()
    .WithContent($"✔ UserMention(user): Hey, {user.Mention}! Listen!")
    .WithAllowedMentions(new IMention[] { new UserMention(user) })
    .SendAsync(ctx.Channel);
```

#### Sending TTS Messages:

```cs
DiscordMessage msg = await new DiscordMessageBuilder()
    .WithContent($"This is a dumb message")
    .HasTTS(true)
    .SendAsync(ctx.Channel);
```

#### Sending an Inline Reply:

```cs
DiscordMessage msg = await new DiscordMessageBuilder()
    .WithContent($"I'm talking to *you*!")
    .WithReply(ctx.Message.Id)
    .SendAsync(ctx.Channel);
```

By default, replies do not mention. To make a reply mention, simply pass true as the second parameter:

```cs
// ...
    .WithReply(ctx.Message.Id, true);
// ...
```

#### Sending buttons or select menus:

```cs
DiscordMessage msg = await new DiscordMessageBuilder()
    .AddActionRowComponent(button1, button2, button3)
    .SendAsync(ctx.Channel);
```

For more information on buttons, see the [dedicated article.](./interactions/buttons.md)

## Components V2

> [!Important]
> The following content cannot be edited on a message with the components V2 flag: `content`, `embeds`, `stickers`. Furthermore, messages cannot be "downgraded" from Components V2, only *upgraded*.

Components V2 is a relatively new addition to existing components, with new component types, new APIs, and entirely new ways of visualizing content. Enabling components V2 is as simple as calling `EnableComponentsV2` on message builder.

There are some things to take in mind with components V2: the biggest one is that once a message is V2, it is *always* V2. This is a deliberate decision by Discord. Furthermore, messages with the V2 components flag (hereon referred to as Components V2/V2 Messages) only support using components (don't worry, you can still display text!) and setting attachments. However, these attachments *must* be referenced by a component (and for good reason!)

V2 Messages have some unique advantages, however:
- Top-level components are no longer restricted!
- Max total components increased from **25 ➜ 40**

Components V2 (specifically, components introduced by Components V2) do not go in action rows. Instead, they are represented as top level components added to the message at the top level using the strongly named methods: `AddSectionComponent`, `AddThumbnailComponent`, etc.

These new components are intended for more composable display and correspondingly serve to display things, rather than the buttons and selects we mentioned earlier. A non-exhaustive list follows:

- `Section Component`
  - Several sections (3) of text with an accessory (either a thumbnail or button 👀).
  - May support more components than just text in the future

- `Text Display Component`
  - A simple display of text, up to 4000 characters (summed across all text in the message)
  - Sections also count toward this, and also have the 4000-character limit.

- `Thumbnail Component`
  - A simple thumbnail, usable in sections

- `Media Gallery Component`
  - A collection of arbitrary media items (`DiscordMediaGalleryItem`)
  - Can be a remote url or a local file referenced via `attachment://my_file.png`

- `File Component`
  - A singular, arbitrary file
  - Also supports urls or `attachment://` attachments
  - Does not support native previews for text files
  - Can be spoilered

- `Separator Component`
  - Acts as a vertical spacer between components
  - Has two sizes, which are equivalent to 1 and 2 lines of text respectively.
  - Invisible by default, but can be set to render as a line (`divider = true`)

- `Container Component`
  - Arguably the coolest component-
  - Acts as a "container" for other components; can be colored like an embed
  - Can also be spoilered, blurring the entire container and components within
  - Holds action rows, and all new V2 components except containers
  - Holds up to 10 components

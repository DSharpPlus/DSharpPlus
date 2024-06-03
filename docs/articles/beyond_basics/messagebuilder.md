---
uid: articles.beyond_basics.messagebuilder
title: Message Builder
---

## Background

Before the message builder was put into place, we had one large method for sending messages along with 3 additional
methods for sending files. This was becoming a major code smell, and it was hard to maintain and add more parameters onto
it. Now we support just sending a simple message, an embed, a simple message with an embed, or a message builder.

## Using the Message Builder

The API Documentation for the message builder can be found at `DiscordMessageBuilder` but here we'll
go over some of the concepts of using the message builder:

### Adding a File

For sending files, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
using fs = new FileStream("ADumbFile.txt", FileMode.Open, FileAccess.Read);

var msg = await new DiscordMessageBuilder()
    .WithContent("Here is a really dumb file that I am testing with.")
    .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } })
    .SendAsync(ctx.Channel);
```

### Adding Mentions

For sending mentions, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"✔ UserMention(user): Hey, {user.Mention}! Listen!")
    .WithAllowedMentions(new IMention[] { new UserMention(user) })
    .SendAsync(ctx.Channel);
```

### Sending TTS Messages

For sending a TTS message, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"This is a dumb message")
    .HasTTS(true)
    .SendAsync(ctx.Channel);
```

### Sending an Inline Reply

For sending an inline reply, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
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

---
uid: beyond_basics_messagebuilder
title: Message Builder
---

##Background
Before the message builder was put into place, we have 1 large method for sending messages along with 3 additional methods for sending files.  This
was becoming a major code smell and was hard to maintain and add more params onto it.  Now we just support sending a simple message, an embed, a simple
message with an embed, or a full fledge message builder.

##Using the Message Builder
The API Documentation for the message builder can be found @DSharpPlus.Entities.DiscordMessageBuilder but here we will go over some concepts of using the
message builder:

###Adding a File:
With sending a file(s), you will have to use the MessageBuilder to construct your message, see example below:

```cs
 using (var fs = new FileStream("ADumbFile.txt", FileMode.Open, FileAccess.Read))
 {
    var msg = await new DiscordMessageBuilder()
        .WithContent("Here is a really dumb file that i am testing with.")
        .WithFiles(new Dictionary<string, Stream>() { { "ADumbFile1.txt", fs } })
        .SendAsync(ctx.Channel);           
}
```
OR

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent("Here is a really dumb file that i am testing with.")
    .WithFile("./ADumbFile.txt")
    .SendAsync(ctx.Channel);
```

###Adding Mentions
With sending a mention(s), you will have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"✔ UserMention(user): Hey, {user.Mention}! Listen!")
    .WithAllowedMentions(new IMention[] { new UserMention(user) })
    .SendAsync(ctx.Channel);      
```

### Sending TTS Messages
With sending a TTS, you will have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"This is a dumb message")
    .HasTTS(true)
    .SendAsync(ctx.Channel);      
```
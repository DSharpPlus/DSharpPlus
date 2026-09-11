---
uid: articles.basics.intents
title: Intents
---

Intents were added to Discord to help the service not have to push so many events to the bots that were not using them. Most intents are "unprivileged", which means your bot can just specify them, but certain intents are "privileged", meaning they may require approval from Discord to use:

## Privileged Intents

The following intents are "privileged", meaning Discord must approve your usecase for them:

- GuildPresences, which controls the events related to user statuses
- GuildMembers, which controls the events related to guild members in any capacity
- MessageContent, which controls the ability to see the content of messages.

On the [Discord Application under the Bot Page][0] you will have to specify if your bot requires Privileged Intents. We recommend having these all enabled at first to ensure the most stability when building your first bot, otherwise you may run into issues when retrieving entities from the library's cache.

![Bot Page][1]

Once your bot reaches 100 servers or 10,000 cumulative users, whichever comes first, you have to apply to Discord to be able to continue to use these events.

> [!WARNING]
> Discord considers usecases that, according to them, can be achieved without the use of privileged intents to not be reasons to request the intent. For example, this means that text commands are notoriously not considered a reason to be granted the message content intent.

## Discord Configuration

Within your setup code you have to specify all intents you need. As mentioned above, we recommend having all intents enabled during initial development, so you should specify @DSharpPlus.DiscordIntents.All in your configuration:

```csharp
DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.All);
```

As your project advances and its functioning becomes clear, you should turn off intents you do not need in order to save resources. Here is an example of just specifying one:

```csharp
DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.GuildMessages);
```

Here is an example of specifying many:

```csharp
DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault
(
    token, 
    DiscordIntents.DirectMessageReactions 
        | DiscordIntents.DirectMessages 
        | DiscordIntents.GuildBans 
        | DiscordIntents.GuildEmojis 
        | DiscordIntents.GuildInvites 
        | DiscordIntents.GuildMembers
        | DiscordIntents.GuildMessages
        | DiscordIntents.Guilds
        | DiscordIntents.GuildVoiceStates 
        | DiscordIntents.GuildWebhooks
};
```

If you specify a privileged intent in your client builder that you don't have access to, an error will be thrown on startup.

<!-- LINKS -->
[0]: https://discord.com/developers/applications
[1]: ../../images/Intents.png
[2]: https://support.discord.com/hc/en-us/articles/360040720412-Bot-Verification-and-Data-Whitelisting
[3]: xref:DSharpPlus.DiscordIntents

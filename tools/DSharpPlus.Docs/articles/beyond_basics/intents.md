---
uid: articles.beyond_basics.intents
title: Intents
---

## Intents
Intents were added to Discord to help the service not have to push so many events to the bots that were not using them.
If you are going to be needing to subscribe to any type of event, they are going to have to be defined **BOTH** within
the [Discord Application under the Bot Page][0] on Discords Site and also within the @DSharpPlus.DiscordConfiguration.

### Discord Application
On the [Discord Application under the Bot Page][0] you will have to specify if your bot requires Privileged Intents. We
recommend having these all enabled at first to ensure the most stability when building your first bot, otherwise you may
run into issues when retrieving entities from the library's cache.

![Bot Page][1]

>[!WARNING]
> These privileged intents may not be available for you to toggle on immediately. 
>
> Due to their nature of sensitive data, Discord requires you to go through a verification process once your bot is in a
> certain amount of servers. Please read this [blog post][2] for more information and how to apply.

### Discord Configuration
Within your @DSharpPlus.DiscordConfiguration you will have to specify all the intents you will need. Here is a list of
all the [Intents][3] DSharpPlus Supports. By default, the configuration will use
@DSharpPlus.DiscordIntents.AllUnprivileged as the default value. Like above however, we recommend having all intents
enabled at first, so you should specify @DSharpPlus.DiscordIntents.All in your configuration which will include the
privleged intents you enabled in your application:
```csharp
var config = new DiscordConfiguration()
{
    Intents = DiscordIntents.All
};
```

When you become more advanced, you can try experimenting with turning off intents you do not need in order to save
resources. In your @DSharpPlus.DiscordConfiguration you can specify one or many.

Here is an example of just specifying one: 
```csharp
var config = new DiscordConfiguration()
{
    Intents = DiscordIntents.GuildMessages
};
```

Here is an example of specifying many:
```csharp
var config = new DiscordConfiguration()
{
    Intents = DiscordIntents.DirectMessageReactions 
    | DiscordIntents.DirectMessages 
    | DiscordIntents.GuildBans 
    | DiscordIntents.GuildEmojis 
    | DiscordIntents.GuildInvites 
    | DiscordIntents.GuildMembers
    | DiscordIntents.GuildMessages
    | DiscordIntents.Guilds
    | DiscordIntents.GuildVoiceStates 
    | DiscordIntents.GuildWebhooks,
};
```

Please Note, if you specify a privileged intent within your @DSharpPlus.DiscordConfiguration that you have not signed up
for on the Discord Application page, an error will be thrown on the connection. 

<!-- LINKS -->
[0]:  https://discord.com/developers/applications
[1]:  ../../images/Intents.png
[2]:  https://support.discord.com/hc/en-us/articles/360040720412-Bot-Verification-and-Data-Whitelisting
[3]:  xref:DSharpPlus.DiscordIntents

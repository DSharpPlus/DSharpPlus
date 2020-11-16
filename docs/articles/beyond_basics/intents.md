---
uid: beyond_basics_intents
title: Intents
---

## Intents
Intents were added to Discord to help the service not have to push so many events to the bots that were not using them.  If you are going to be needing
to subscribe to any type of event, they are going to have to be defined **BOTH** within the [Discord Application under the Bot Page](https://discord.com/developers/applications) on Discords Site and also within the @DSharpPlus.DiscordConfiguration.  


### Discord Application
On the [Discord Application under the Bot Page](https://discord.com/developers/applications) you will have to specify if your bot requires Privileged Intents.  
![Bot Page](/images/Intents.png)

### Discord Configuration
Within your Discord Configuration you will have to specify all the Intents you will be needing.  You can specify just one or many.  Here is a listing of all the
[Intents](xref:DSharpPlus.DiscordIntents) DSharpPlus Supports

Here is an example of just specifying one: 
```csharp
var config = new DiscordConfiguration()
{
    Intents = DiscordIntents.GuildMessages
};
```

Here is an example of specifing many:

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


Please Note, if you specify a Privileged Intent within your Discord Configuration that you have not signed up for on the Discord Application page, an error will be thrown on the Connection. 
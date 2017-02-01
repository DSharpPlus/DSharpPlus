DiscordWebhook
==============
Represents a webhook

## Members

`ulong ID`: ID for this object

`DateTime CreationDate`: When this was created

`ulong GuildID`: The guild ID this webhook is for

`ulong ChannelID`: The channel ID this webhook is for

`DiscordUser User`: The user that created this webhook

`string Name`: This webhook's name

`string Avatar`: Default avatar for this webhook

`string Token`: Secure token for this webhook

## Methods
#### Modify
Modifies this webhook

`string name = ""`: New name for this webhook

`string base64avatar = ""`: New avatar for this webhook (base64)

Returns: `DiscordWebhook`

#### Delete
Deletes this webhook

Returns: Nothing

#### Execute
Executes this webhook

`string content = ""`: Content for the message

`string username = ""`: Username for the message

`string avatarurl = ""`: Avatar for the message

`bool tts = false`: Wether the message will be tts

`List<DiscordEmbed> embeds = null`: List of embeds to ship with this message

Returns: Nothing

#### ExecuteSlack
Executes this webhook with a slack webhook payload

`string json`: payload to send to the webhook

Returns: Nothing

#### ExecuteGithub
Executes this webhook with a github webhook payload

`string json`: payload to send to the webhook

Returns: Nothing

MessageCreated
==============
Gets sent when a message gets created

## EventArgs:
`DiscordMessage Message`: Message object that got created

`List<DiscordMember> MentionedUsers`: Users that got mentioned

`List<DiscordRole> MentionedRoles`: Roles that got mentioned

`List<DiscordChannel> MentionedChannels`: Channels that got mentioned

`List<DiscordEmoji> UsedEmojis`: Emojis that were used

`DiscordChannel Channel`: Channel message was sent in

`DiscordGuild Guild`: Guild message was sent in

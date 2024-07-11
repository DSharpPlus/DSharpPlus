---
uid: articles.commands.command_processors.slash_commands.missing_commands
title: Missing Commands
---

# Missing Commands

#### Help! I registered all my slash commands but they aren't showing up!

When the Discord App (the client you use, not the bot) starts up, it fetches all the commands that are registered with the bot and caches them to a Discord channel.

>[!IMPORTANT]
> This cache is only populated at startup, so updating commands requires reloading your app (`ctrl + r` on desktop, or re-launching the app on mobile)

#### Help! They're still not showing up!

There are many ways a command can violate the given constraints (For SlashCommands discords naming constraints or a missing type converter for example).
In this case the extensions throws an exception, dont register any commands and logs the exception with a descriptive message


If you have text commands enabled, try mentioning your bot with the command name and see if it responds. 
If it doesn't, then the command is not registered. If it does you are likely missing the `MessageContent` intent.

If you're still having issues, try checking the bot's logs. 
The DSharpPlus library will log any errors that occur when registering commands.
If you see any errors, try fixing them and re-registering the commands.
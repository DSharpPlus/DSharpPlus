---
uid: articles.commands.command_processors.slash_commands.missing_commands
title: Missing Commands
---

# Missing Commands

> Help! I registered all my slash commands but they aren't showing up!

When the Discord App (the client you use, not the bot) starts up, it fetches all the commands that are registered with the bot and caches them to a Discord channel. Unfortunately this cache is not properly updated in real-time, so it may take a while for the commands to show up.

Your solution here is to reload your Discord App.

> Help! They're still not showing up!

Maybe some of your commands were invalid and the DSharpPlus.Commands library didn't register them. If you have text commands enabled, try mentioning your bot with the command name and see if it responds. If it doesn't, then the command is not registered.

If you're still having issues, try checking the bot's logs. The DSharpPlus library will log any errors that occur when registering commands. If you see any errors, try fixing them and re-registering the commands.
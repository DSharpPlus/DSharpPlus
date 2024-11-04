---
uid: articles.commands.command_processors.slash_commands.missing_commands
title: Missing Commands
---

# Missing Commands

#### Help! I registered all my slash commands but they aren't showing up!

When the Discord App (the client you use, not the bot) starts up, it fetches all the commands that are registered with each bot and caches them to the current Discord channel. This means that if you register a command while the Discord App is running, you won't see the command until you restart the Discord App (`Ctrl + R`).

#### Help! They're still not showing up!

Some slash commands may be missing if they don't follow the requirements that Discord has set. First and foremost, always check your logs for errors. If a command parameter doesn't have a type converter, has a name/description that's too long or other miscellaneous issues, the Commands framework will avoid registering that specific command and print an error into the console.

There should never be a case when a command is silently skipped. If you're experiencing this issue, double check that the command is being registered correctly and that there are no errors in the logs. If you're still having trouble, feel free to open up a GitHub issue or a help post in the [Discord server](https://discord.gg/dsharpplus).
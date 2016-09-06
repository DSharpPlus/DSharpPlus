# SharpCord_Starter
[Discord.NET version](https://github.com/NaamloosDT/Discord.NET_Starter)
### Introduction
So YOU want to make a discord bot of your own? Look no further! This is exactly what you need! SharpCord_Starter is a starter project to get you started at making your very own Discord bot using SharpCord! I myself am currently using the latest version of sharpCord for this project, but you can actually use any version you'd like! *(you might have to change some stuff due to library changes.)* 

### SharpCord
I hear you asking, what exactly is SharpCord? SharpCord is an AMAZING discord bot library Based off of the DiscordSharp library made by LuigiFan. You can download it [here](https://github.com/NaamloosDT/SharpCord). It's fairly easy to use and a good way to get you into bot development!

### My Discord server
And on top of all of that, If you need any help i would love to help you out! just join me at my personal [discord server!](https://discord.gg/0oZpaYcAjfvkDuE4) and ask anything you'd like! But please, DON'T add any bots without my permission. it's kinda rude :)

### Functions
And i hear you asking, what does this starter bot do? Well, I already predicted you wouyd ask that! so i made a nice list of functions :)

- [x] Check if a channel message has been received, and respond to that
- [x] Check if a private message has been received, and respond to that
- [x] Join a channel after receiving a "join *inviteurl*" private message (DOESN'T WORK WITH API)
- [x] Send a message once a new channel has been created
- [x] Welcome a user to your server (in private chat)
- [x] Block message deleting
- [x] Check if user has correct role
- [x] Kinda hardcoded random.cat sender
- [x] Add code on how library works with the API


### Joining servers
A bot can join a server by sending `join [inviteurl]` to it.

however, the API does not support that. To make a bot join using the api, use the following URL:
`https://discordapp.com/oauth2/authorize?&client_id=[YOUR_CLIENT_ID]&scope=bot&permissions=0`

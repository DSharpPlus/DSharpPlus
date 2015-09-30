# DiscordSharp
A C# API for Discord.

#how do use

Discord is what I like to call, an *event based* client. In other words, you get your instance of your client and hook up to its various events: either by lambda or by delegating to external voids. A simple example is as follows..

```
using(var client = new DiscordClient())
{
  client.ClientPrivateInformation.email = "email";
  client.ClientPrivateInformation.password = "pass";
  
  client.Connected += (sender, e) =>
  {
    Console.WriteLine("Connected! User: " + e.username);
  };
  client.SendLoginRequest();
  Thread t = new Thread(client.ConnectAndReadMessages);
  t.Start();
}
```
This will get you logged in, and print out a login notification to the console with the username you've logged in as.

#Notes
* This is in such beta it's not even funny.
* All of the internal classes are meant to model Discord's internal Json. This is why DiscordMember has a subset, user with the actual information.

#Cousins
We're all one big happy related family. 

Discord.Net, another great C# library - https://github.com/RogueException/Discord.Net

Discord4J, a Java library - https://github.com/racist/Discord4J

discord.io, a Node.js library which I referenced a lot - https://github.com/izy521/discord.io

discord.js, an alternate Node.js library - https://github.com/discord-js/discord.js

DiscordPHP, a PHP library - https://github.com/teamreflex/DiscordPHP

discordrb, a Ruby library - https://github.com/meew0/discordrb

discord.py, a Python library - https://github.com/Rapptz/discord.py

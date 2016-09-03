![Logo of SharpCord](https://github.com/NaamloosDT/SharpCord/blob/master/logo.png)

# SharpCord

A C# API for Discord based off [DiscordSharp](https://github.com/suicvne/DiscordSharp) :3 

#How to use

```
DiscordClient client = new DiscordClient();
client.ClientPrivateInformation.Email = "email";
client.ClientPrivateInformation.Password = "pass";

client.Connected += (sender, e) =>
{
  Console.WriteLine($"Connected! User: {e.User.Username}");
};
client.SendLoginRequest();
Thread t = new Thread(client.Connect);
t.Start();
```

#Update from Discordsharp

Change your `using DiscordSharp` to `using SharpCord`

##Example Bot (might be a bit outdated)
* https://github.com/NaamloosDT/DiscordSharp_Starter 

##Questions?
Come talk to us [here](http://www.discord.gg/h7mJ5x)! <3

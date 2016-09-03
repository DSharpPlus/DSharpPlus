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

Change your `using Discordsharp` to `using Shar

##Example Bot (might be a bit outdated)
* https://github.com/NaamloosDT/DiscordSharp_Starter 

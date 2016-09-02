# SharpCord

A C# API for Discord based off (DiscordSharp)[https://github.com/suicvne/DiscordSharp] :3 

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

This will get you logged in and print out a login notification to the console with the username you've logged in as.

##Example Bot (might be a bit outdated)
* https://github.com/NaamloosDT/DiscordSharp_Starter 

# DiscordSharp
A C# API for Discord.

#how do use

Discord is what I like to call, an *event based* client. In other words, you get your instance of your client and hook up to its various events: either by lambda or by delegating to external voids. A simple example is as follows..

```
using(var client = new DiscordClient())
{
  client.LoginInformation.email[0] = "email"; //this is because Discord accepts a string array for some reason...
  client.LoginInformation.password[0] = "pass";
  
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

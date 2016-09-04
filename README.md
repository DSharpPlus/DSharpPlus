![Logo of SharpCord](https://github.com/NaamloosDT/SharpCord/blob/master/logo_smaller.png)

# SharpCord

A C# API for Discord based off [DiscordSharp](https://github.com/suicvne/DiscordSharp) :3 

# Getting Started

## 1. Installation and setup

### 1.1 Git

First start off by downloading the git repo

`git clone https://github.com/NaamloosDT/SharpCord.git`

Then open the project in Visual Studio, and build the class library.
After you have done that, make a new C# console program project. Set the class library as a reference. Then use move to 2.


## 2. Connecting
Start your bot off with this code.
```
using SharpCord;

class Program {
  void Main() {
    DiscordClient client = new DiscordClient();
    client.ClientPrivateInformation.Email = "email";
    client.ClientPrivateInformation.Password = "pass";

    client.Connected += (sender, e) =>
    {
      Console.WriteLine($"Connected! User: {e.User.Username}");
    };
  }

  client.SendLoginRequest();
  Thread t = new Thread(client.Connect);
  t.Start();
}
```
In a nutshell, the code 

# Documentation
* https://naamloosdt.github.io/SharpCord/

# Old Example Bot (might be a bit outdated)
* https://github.com/NaamloosDT/DiscordSharp_Starter 

# Questions?
Come talk to us [here](http://www.discord.gg/h7mJ5x)! :heart:

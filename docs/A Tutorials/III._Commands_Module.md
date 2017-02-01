The Commands Module
===================
Please note: The Commands Module is WIP and very basic.

## Installation
Currently, the commands module is separate from the main module. Also, unlike the main module there is no NuGet package for it yet.

### Compiling it yourself
1. Download the Git repository
2. Open the project in Visual Studio
3. Build
4. Open your bot's project
5. Refrence the built DLLs ("DSharpPlus.Commands.dll")
6. Done!

## Using the commands module
Let's start from the code from "Your first message"
```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Voice;

namespace DSharpPlusBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordClient _client = new DiscordClient(new DiscordConfig()
            {
                Token = File.ReadAllText("token.txt"),
                AutoReconnect = true
            });

            _client.MessageCreated += async (sender, e) =>
            {

                if (!e.Message.Author.IsBot)
                {
                    if (e.Message.Content.StartsWith("!hello"))
                        await e.Channel.SendMessage($"Hello, {e.Message.Author.ID}");
                }
            };

            _client.Connect();

            Console.ReadLine();
        }
    }
}
```
Now, similar to DiscordConfig, we are going to use CommandConfig and define the new Commands module of our bot.
```cs
_client.UseCommands(new CommandConfig()
{
  Prefix = "!",
  SelfBot = false
});
```

Now, `Prefix` is what will be in front of every command. For example, the "hello" command will be "!hello" now.

`SelfBot` makes it so that only the bot's user can use commands. This is useful if you are adding a bot to your user account.

## Your first command
Adding commands is easy. First, let's create a new method called "CreateCommands" and call it in our Main() method.
```cs
  Main() code...
  CreateCommands(_client);
}
CreateCommands(DiscordClient _client) {

}
```
Creating commands in another method isn't required, but it is what we will be using for this tutorial.

Now, inside the CreateCommands method, lets add a command called "hello"
```cs
void CreateCommands(DiscordClient _client)
{
  _client.AddCommand("hello", async (e) =>
  {

  });
}
```
Now, we can't just move the code from the MessageCreated event to the new command, there is a little change we need to make. Instead of `e.Channel.SendMessage` we must use `e.Message.Parent.SendMessage`. Yiur final code should look similar to this:
```cs
void CreateCommands(DiscordClient _client)
{
  _client.AddCommand("hello", async (e) =>
  {
    await e.Message.Parent.SendMessage($"Hello, {e.Message.Author.Mention}");
  });
}
```
Now, compile and run your bot. Try out the new command by putting in "!hello" into chat. You should receive a message back similar to "Hello, @username".
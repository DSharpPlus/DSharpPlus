Your First Message
==================

## Initial code
We are going to use the code from the main page in this tutorial.
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

            _client.Connect();

            Console.ReadLine();
        }
    }
}
```

## Sending your first message
Now, under the definition of your DiscordClient, you should add a MessageCreated event.
```cs
_client.MessageCreated += async (sender, e) =>
{

};
```
This happens whenever a user posts a message on a channel that the bot can see, the server doesn't matter.

Now, create an if statement to see if the user isn't a bot.
```cs
if (!e.Message.Author.IsBot) {

}
```
This makes it so that the bot ignores messages posted by other bots, including itself.

Now, let's make another if statement withen that one, this time it will be to see if the message's text started with "!hello".

```cs
if (e.Message.Content.StartsWith("!hello")) {

}
```
Inside that if statement, we will ahve the bot send a message. This line of code is simple. `await e.Channel.SendMessage($"Hello, <@{e.Message.Author.ID}>")`. This makes the bot respond to the user with "Hello, @username". **Please note: In versions 1.1 and above, this should be replaced with `$"Hello, {e.Message.Author.Mention}"`**

Now, compile and run the bot. In a channel that both you and the bot can post in, type "!hello" and you should receive a message back.

## Where to from here
Proceed to "Commands Module" page.
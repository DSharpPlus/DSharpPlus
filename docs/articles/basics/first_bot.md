---
uid: articles.basics.first_bot
title: Your First Bot
---

# Your First Bot
>[!NOTE] 
> This article assumes the following:
> * You have [created a bot account][0] and have a bot token.
> * You have [Visual Studio 2019][1] installed on your computer.
 
## Create a Project
Open up Visual Studio and click on `Create a new project` towards the bottom right.

![Visual Studio Start Screen][2]

Select `Console App (.NET Core)` then click on the `Next` button.

![New Project Screen][3]

Next, you'll give your project a name. For this example, we'll name it `MyFirstBot`. If you'd like, you can also change
the directory that your project will be created in.

Enter your desired project name, then click on the `Create` button.

![Name Project Screen][4]

Voilà! Your project has been created!

![Visual Studio IDE][5]

## Install Package
Now that you have a project created, you'll want to get DSharpPlus installed.
Locate the *solution explorer* on the right side, then right click on `Dependencies` and select `Manage NuGet Packages`
from the context menu.

![Dependencies Context Menu][6]

You'll then be greeted by the NuGet package manager.

Select the `Browse` tab towards the top left, then type `DSharpPlus` into the search text box.

![NuGet Package Search][7]

The first results should be the six DSharpPlus packages.

![Search Results][8]

Package                    | Description
:-------------------------:|:---:
`DSharpPlus`               | Main package; Discord API client.
`DSharpPlus.CommandsNext`  | Add-on which provides a command framework.
`DSharpPlus.SlashCommands` | Add-on which provides an application command framework.
`DSharpPlus.Interactivity` | Add-on which allows for interactive commands.
`DSharpPlus.Lavalink`      | Client implementation for [Lavalink][9]. Useful for music bots.
`DSharpPlus.VoiceNext`     | Add-on which enables connectivity to Discord voice channels.
`DSharpPlus.Rest`          | REST-only Discord client.

We'll only need the `DSharpPlus` package for the basic bot we'll be writing in this article. Select it from the list
then click the `Install` button to the right (after verifing that you will be installing the **latest version**).

![Install DSharpPlus][10]

You're now ready to write some code!

## First Lines of Code
DSharpPlus implements [Task-based Asynchronous Pattern][11]. Because of this, the majority of DSharpPlus methods must be
executed in a method marked as `async` so they can be properly `await`ed.

Due to the way the compiler generates the underlying [IL][12] code, marking our `Main` method as `async` has the
potential to cause problems. As a result, we must pass the program execution to an `async` method.

Head back to your *Program.cs* tab and empty the `Main` method by deleting line 9.

![Code Editor][13]

Now, create a new `static` method named `MainAsync` beneath your `Main` method. Have it return type `Task` and mark it
as `async`. After that, add `MainAsync().GetAwaiter().GetResult();` to your `Main` method.
```cs
static void Main(string[] args)
{
    MainAsync().GetAwaiter().GetResult();
}

static async Task MainAsync()
{
    
}	
```

If you typed this in by hand, Intellisense should have generated the required `using` directive for you. However, if you
copy-pasted the snippet above, VS will complain about being unable to find the `Task` type.

Hover over `Task` with your mouse and click on `Show potential fixes` from the tooltip.

![Error Tooltip][14]

Then apply the recommended solution.

![Solution Menu][15]

We'll now create a new `DiscordClient` instance in our brand new asynchronous method.

Create a new variable in `MainAsync` and assign it a new @DSharpPlus.DiscordClient instance, then pass an instance of
@DSharpPlus.DiscordConfiguration to its constructor. Create an object initializer for @DSharpPlus.DiscordConfiguration
and populate the @DSharpPlus.DiscordConfiguration.Token property with your bot token then set the
@DSharpPlus.DiscordConfiguration.TokenType property to @DSharpPlus.TokenType.Bot. Next add the
@DSharpPlus.DiscordClient.Intents property and populated it with the @DSharpPlus.DiscordIntents.AllUnprivileged value.
These Intents are required for certain Events to be fired. Please visit this [article][16] for more information.
```cs
var discord = new DiscordClient(new DiscordConfiguration()
{
    Token = "My First Token",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged     
});
```

>[!WARNING]
> We hard-code the token in the above snippet to keep things simple and easy to understand.
>
> Hard-coding your token is *not* a smart idea, especially if you plan on distributing your source code.
> Instead you should store your token in an external medium, such as a configuration file or environment variable, and
> read that into your program to be used with DSharpPlus.

Follow that up with @DSharpPlus.DiscordClient.ConnectAsync* to connect and login to Discord, and `await Task.Delay(-1);`
at the end of the method to prevent the console window from closing prematurely.
```cs
var discord = new DiscordClient();
	
await discord.ConnectAsync();
await Task.Delay(-1);
``` 

As before, Intellisense will have auto generated the needed `using` directive for you if you typed this in by hand. If
you've copied the snippet, be sure to apply the recommended suggestion to insert the required directive.

If you hit `F5` on your keyboard to compile and run your program, you'll be greeted by a happy little console with a
single log message from DSharpPlus. Woo hoo!

![Program Console][17]

## Spicing Up Your Bot
Right now our bot doesn't do a whole lot. Let's bring it to life by having it respond to a message!

As of September 1st 2022, Discord started requiring message content intent for bots that want to read message content. This is a privileged intent!

If your bot has under 100 guilds, all you have to do is flip the switch in the developer dashboard. (over at https://discord.com/developers/applications)
If your bot has over 100 guilds, you'll need approval from Discord's end.

After enabling the intent on Discords end, you have to specify your intents in you DiscordConfiguratioon:

```cs
var discord = new DiscordClient(new DiscordConfiguration()
{
    Token = "My First Token",
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents  
});
```

Now you can start to listen to messages.

Hook the @DSharpPlus.DiscordClient.MessageCreated event fired by @DSharpPlus.DiscordClient with a [lambda][18]. Mark it
as `async` and give it two parameters: `s` and `e`.
```cs
discord.MessageCreated += async (s, e) =>
{
    
};
```
 
Then, add an `if` statement into the body of your event lambda that will check if
@DSharpPlus.Entities.DiscordMessage.Content starts with your desired trigger word and respond with a message using
@DSharpPlus.Entities.DiscordMessage.RespondAsync* if it does. For this example, we'll have the bot to respond with
*pong!* for each message that starts with *ping*.
```cs
discord.MessageCreated += async (s, e) =>
{
    if (e.Message.Content.ToLower().StartsWith("ping")) 
		await e.Message.RespondAsync("pong!");
};
```

## The Finished Product
Your entire program should now look like this:

```cs
using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace MyFirstBot
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "My First Token",
                TokenType = TokenType.Bot,
		Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });

            discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping")) 
                    await e.Message.RespondAsync("pong!");

            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
```

Hit `F5` to run your bot, then send *ping* in any channel your bot account has access to. Your bot should respond with
*pong!* for each *ping* you send.

Congrats, your bot now does something!

![Bot Response][19]

## Further Reading
Now that you have a basic bot up and running, you should take a look at the following:

* [Events][20]
* [CommandsNext][21]

<!-- LINKS -->
[0]:  xref:articles.basics.bot_account "Creating a Bot Account"
[1]:  https://visualstudio.microsoft.com/vs/
[2]:  ../../images/basics_first_bot_01.png
[3]:  ../../images/basics_first_bot_02.png
[4]:  ../../images/basics_first_bot_03.png
[5]:  ../../images/basics_first_bot_04.png
[6]:  ../../images/basics_first_bot_05.png
[7]:  ../../images/basics_first_bot_06.png
[8]:  ../../images/basics_first_bot_07.png
[9]:  xref:articles.audio.lavalink.setup
[10]: ../../images/basics_first_bot_08.png
[11]: https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern
[12]: https://en.wikipedia.org/wiki/Common_Intermediate_Language
[13]: ../../images/basics_first_bot_09.png
[14]: ../../images/basics_first_bot_10.png
[15]: ../../images/basics_first_bot_11.png
[16]: xref:articles.beyond_basics.intents
[17]: ../../images/basics_first_bot_12.png
[18]: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions
[19]: ../../images/basics_first_bot_13.png
[20]: xref:articles.beyond_basics.events
[21]: xref:articles.commands.intro

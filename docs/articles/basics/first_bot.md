---
uid: basics_first_bot
title: Your First Bot
---

# Your First Bot
 >[!NOTE] 
 > This article assumes the following:
 > * You have [created a bot account](xref:basics_bot_account "Creating a Bot Account") and have a bot token.
 > * You have [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) installed on your computer.
 

## Create a Project
Open up Visual Studio and click on `Create a new project` towards the bottom right.

![Visual Studio Start Screen](/images/basics_first_bot_01.png)

<br/>
Select `Console App (.NET Core)` then click on the `Next` button.

![New Project Screen](/images/basics_first_bot_02.png)

<br/>
Next, you'll give your project a name. For this example, we'll name it `MyFirstBot`.<br/>
If you'd like, you can also change the directory that your project will be created in.

Enter your desired project name, then click on the `Create` button.

![Name Project Screen](/images/basics_first_bot_03.png)

<br/>
Voilà! Your project has been created!

![Visual Studio IDE](/images/basics_first_bot_04.png)


## Install Package
Now that you have a project created, you'll want to get DSharpPlus installed.
Locate the *solution explorer* on the right side, then right click on `Dependencies` and select `Manage NuGet Packages` from the context menu.

![Dependencies Context Menu](/images/basics_first_bot_05.png)

<br/>
You'll then be greeted by the NuGet package manager.

Select the `Browse` tab towards the top left, then type `DSharpPlus` into the search text box with the Pre-release checkbox checked **ON**.

![NuGet Package Search](/images/basics_first_bot_06.png)

<br/>
The first results should be the six DSharpPlus packages.

![Search Results](/images/basics_first_bot_07.png)
Package|Description
:---: |:---:
`DSharpPlus`|Main package; Discord API client.
`DSharpPlus.CommandsNext`|Add-on which provides a command framework.
`DSharpPlus.Interactivity`|Add-on which allows for interactive commands.
`DSharpPlus.Lavalink`|Client implementation for [Lavalink](xref:audio_lavalink_setup). Useful for music bots.
`DSharpPlus.VoiceNext`|Add-on which enables connectivity to Discord voice channels.
`DSharpPlus.Rest`|REST-only Discord client.

<br/>
We'll only need the `DSharpPlus` package for the basic bot we'll be writing in this article.<br/>
Select it from the list then click the `Install` button to the right (after verifing that you will be installing the **latest 4.0 version**).

![Install DSharpPlus](/images/basics_first_bot_08.png)

You're now ready to write some code!


## First Lines of Code
DSharpPlus implements [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern).
Because of this, the majority of DSharpPlus methods must be executed in a method marked as `async` so they can be properly `await`ed.

Due to the way the compiler generates the underlying [IL](https://en.wikipedia.org/wiki/Common_Intermediate_Language) code, 
marking our `Main` method as `async` has the potential to cause problems. As a result, we must pass the program execution to an `async` method.

Head back to your *Program.cs* tab and empty the `Main` method by deleting line 9.

![Code Editor](/images/basics_first_bot_09.png)

Now, create a new `static` method named `MainAsync` beneath your `Main` method. Have it return type `Task` and mark it as `async`.
After that, add `MainAsync().GetAwaiter().GetResult();` to your `Main` method.

```cs
static void Main(string[] args)
{
    MainAsync().GetAwaiter().GetResult();
}

static async Task MainAsync()
{
    
}	
```

If you typed this in by hand, Intellisense should have generated the required `using` directive for you.<br/>
However, if you copy-pasted the snippet above, VS will complain about being unable to find the `Task` type.

Hover over `Task` with your mouse and click on `Show potential fixes` from the tooltip.

![Error Tooltip](/images/basics_first_bot_10.png)

Then apply the recommended solution.

![Solution Menu](/images/basics_first_bot_11.png)

<br/>
We'll now create a new `DiscordClient` instance in our brand new asynchronous method.

Create a new variable in `MainAsync` and assign it a new `DiscordClient` instance, then pass an instance of `DiscordConfiguration` to its constructor.
Create an object initializer for `DiscordConfiguration` and populate the `Token` property with your bot token then set the `TokenType` property to `TokenType.Bot`.
Next add the `Intents` Property and Populated it with the @DSharpPlus.DiscordIntents.AllUnprivileged value. These Intents 
are required for certain Events to be fired.  Please visit this [article](xref:beyond_basics_intents) for more information.

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
 > Instead you should store your token in an external medium, such as a configuration file or environment variable, and read that into your program to be used with DSharpPlus.

Follow that up with `await discord.ConnectAsync();` to connect and login to Discord, and `await Task.Delay(-1);` at the end of the method to prevent the console window from closing prematurely.
```cs
var discord = new DiscordClient();
	
await discord.ConnectAsync();
await Task.Delay(-1);
``` 
As before, Intellisense will have auto generated the needed `using` directive for you if you typed this in by hand.<br/>
If you've copied the snippet, be sure to apply the recommended suggestion to insert the required directive.

If you hit `F5` on your keyboard to compile and run your program, you'll be greeted by a happy little console with a single log message from DSharpPlus. Woo hoo!

![Program Console](/images/basics_first_bot_12.png)


## Spicing Up Your Bot
Right now our bot doesn't do a whole lot. Let's bring it to life by having it respond to a message!

Hook the `MessageCreated` event fired by `DiscordClient` with a 
[lambda](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions).<br/>
Mark it as `async` and give it two parameters: `s` and `e`.

```cs
discord.MessageCreated += async (s, e) =>
{
    
};
```
 
Then, add an `if` statement into the body of your event lambda that will check if `e.Message.Content` starts with your desired trigger word and respond with 
a message using `e.Message.RespondAsync` if it does. For this example, we'll have the bot to respond with *pong!* for each message that starts with *ping*.

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
                TokenType = TokenType.Bot
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

Hit `F5` to run your bot, then send *ping* in any channel your bot account has access to.<br/>
Your bot should respond with *pong!* for each *ping* you send.

Congrats, your bot now does something!

![Bot Response](/images/basics_first_bot_13.png)


## Further Reading
Now that you have a basic bot up and running, you should take a look at the following:

* [Events](xref:beyond_basics_events)
* [CommandsNext](xref:commands_intro)

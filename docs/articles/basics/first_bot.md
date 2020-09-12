---
uid: basics_first_bot
title: Your First Bot
---

# Your First Bot
 >[!IMPORTANT] 
 > This article assumes you have [created a bot account](xref:basics_bot_account "Creating a Bot Account") and have a bot token.<br/>
 > It is also assumed that [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) is installed on your computer.

## Create a Project
Open up Visual Studio and click on `Create a new project` towards the bottom right.

![Visual Studio Start Screen](/images/02_01_start_screen.png "Create a new project")

<br/>
Select `Console App (.NET Core)` then click on the `Next` button.

![New Project Screen](/images/02_02_new_project.png "Console App (.NET Core)")

<br/>
Next, you'll give your project a name. In this example, we'll name it `MyFirstBot`.<br/>
If desired, you can also change the directory that your project will be created in.

Enter your desired project name, then click on the `Create` button.

![Name Project Screen](/images/02_03_name_project.png "Naming your project")

<br/>
Voilà! Your project has been created!

![Visual Studio IDE](/images/02_04_voila.png "Your new project!")


## Install Package
 >[!NOTE]
 > The latest version on NuGet (3.2.3) contains several broken features due in part to changes by Discord.
 >
 > It is highly recommended that you instead use the latest nightly build of version 4.0.0.<br/>
 > Check out the [nightly builds](xref:misc_nightly_builds) article for instructions on adding the nightly [SlimGet](https://github.com/Emzi0767/SlimGet) feed.
 >
 > The remainder of this article will assume you are using a nightly build.

Now that you have a project created, you'll want to get DSharpPlus installed.

On the right, you'll see the *solution explorer*. It displays your project with all of its dependencies and files.<br/>
Right click on `Dependencies` and select `Manage NuGet Packages...` from the context menu.

![Dependencies Context Menu](/images/02_05_context_menu.png "Dependencies Context Menu")

<br/>
You'll then be greeted by the NuGet package manager.

Select the `Browse` tab towards the top left, then type `DSharpPlus` into the search text box.

![NuGet Package Search](/images/02_06_browse_nuget.png "NuGet Package Search")

<br/>
The first results should be the six DSharpPlus packages.

![Search Results](/images/02_07_search_results.png "Search Results")
Package|Description
:---: |:---:
`DSharpPlus`|Main package; Discord API client.
`DSharpPlus.CommandsNext`|Add-on which provides a command framework.
`DSharpPlus.Interactivity`|Add-on which allows for interactive commands.
`DSharpPlus.Lavalink`|Client implementation for [Lavalink](xref:audio_lavalink_intro). Useful for music bots.
`DSharpPlus.VoiceNext`|Add-on which enables connectivity to Discord voice channels.
`DSharpPlus.Rest`|REST-only Discord client.

<br/>
We'll only need the `DSharpPlus` package for the basic bot we'll be writing in this article.<br/>
Select it from the list then click the `Install` button to the right.

![Install DSharpPlus](/images/02_08_dsharplus.png "Install DSharpPlus")

You're now ready to write some code!


## First Lines of Code
Close the *NuGet: MyFirstBot* tab near the top left and head back to the *Program.cs* tab.<br/>

![Close NuGet Tab](/images/02_09_close_tab.png "Close Tab")

Once you're there, empty the `Main` method by deleting `Console.WriteLine("Hello World!");` on line 9.

![Code Editor](/images/02_10_goodbye_world.png "Code Editor")

<br/>
As mentioned elsewhere, DSharpPlus implements [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/consuming-the-task-based-asynchronous-pattern).
Because of this, many DSharpPlus methods must be executed in a method marked as `async` so they can be properly `await`ed.

Since we cannot mark the entry method as `async`, we must pass the program execution to an `async` method.

Create a new `static` method named `MainAsync` beneath your `Main` method.
Have it return type `Task` and mark it as `async`.
After that add `MainAsync().GetAwaiter().GetResult();` to your `Main` method.

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

![Error Tooltip](/images/02_11_display_suggestion.png "Error Tooltip")

Then apply the recommended solution.

![Solution Menu](/images/02_12_apply_suggestion.png "Solution Menu")

<br/>
We'll now create a new `DiscordClient` instance in our brand new asynchronous method.

In `MainAsync`, create a new variable and assign it a new `DiscordClient` instance that has a new instance of
`DiscordConfiguration` passed to its constuctor. Create an initializer for `DiscordConfiguration` then populate 
the `Token` property with your bot token and set the `TokenType` property to `TokenType.Bot`.

Follow that up with `await discord.ConnectAsync();` to connect and login to Discord, and `await Task.Delay(-1);` at the end of the method to prevent the console window from closing prematurely.

```cs
static async Task MainAsync()
{
    var discord = new DiscordClient(new DiscordConfiguration()
    {
        Token = "My First Token",
        TokenType = TokenType.Bot		
    });
	
	await discord.ConnectAsync();
	await Task.Delay(-1);
}
```

 >[!WARNING]
 > In the above snippet, we hard-code our token to keep things simple.
 >
 > Hard-coding your token is not a smart idea, especially if you plan on distributing your source code.
 > You should instead store your token in an external medium, such as a configuration file or envirionment variable, and read that into your program to use with DSharpPlus.
 
As before, Intellisense will have auto generated the needed `using` directive for you if you typed this in by hand.<br/>
If you've copied the snippet, be sure to apply the recommended suggestion to insert the required directive.

## Spicing Up Your Bot
Right now our bot doesn't do a whole lot. Let's bring it to life by having it respond to a message!

Hook the `MessageCreated` event fired by `DiscordClient` with a 
[lambda](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions).<br/>
Mark it as as `async` and give it a parameter named `e`.
 
Then, add an `if` statement into the body of your event lambda that will check if `e.Message.Content` starts with your desired trigger word and respond with a message
if it does. For this example, we'll have the bot to respond with *pong!* for each message that starts with *ping*.

Be sure to slot this in just before `await discord.ConnectAsync();`.

```cs
discord.MessageCreated += async e =>
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

            discord.MessageCreated += async e =>
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

![Bot Response](/images/02_15_its_alive.png "It Lives!")


## Next Steps
Now that you have an basic bot up and running, you'll want to learn how to properly [use async events](xref:beyond_basics_event_reference).

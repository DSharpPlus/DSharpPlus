# Talking with the bot - adding commands for enhanced user interaction

You now have a basic bot. In order to allow users to interact with it easily, you will want to add some commands to it. 
This is done using the CommandsNext module, which will not only handle, registering and executing commands for you, but 
it will also make grabbing additional data for your commands easy.

## 1. Before we continue

Right now, your bot is console-mute. Let's change it. Let's make it output all information about its state and doings.

To do that, add the following options to your `DiscordConfiguration`:

```cs
UseInternalLogHandler = true,
LogLevel = LogLevel.Debug
```

## 2. Installing CommandsNext

Using the procedures in the previous article, install a NuGet package called `DSharpPlus.CommandsNext`.

Now you need to enable CommandsNext extension on your DiscordClient. Add a new field to your bot's `Program` class: 
`static CommandsNextExtension commands;`

Visual Studio will complain, you also need to add `using DSharpPlus.CommandsNext;` to your usings.

Before you connect, enable the module on your client:

```cs
commands = discord.UseCommandsNext(new CommandsNextConfiguration
{
	StringPrefix = ";;"
});
```

This will enable the module, and use `;;` as the command prefix for your bot.

## 3. Creating a command module

First, you need to create a new class to hold your commands. In this example, we'll call it `MyCommands`.

Once it's created, you should be presented with a file that looks like this:

```cs
using System;
using System.Collections.Generic;
using System.Text;

namespace MyFirstBot
{
    class MyCommands
    {
    }
}
```

Add a `public` modifier to the class. That class will now serve as your command module.

Before you can proceed, add `using System.Threading.Tasks;`, `using DSharpPlus;`, 
`using DSharpPlus.CommandsNext;`, and `using DSharpPlus.CommandsNext.Attributes;` to the using section.

Go back to your main bot class, and below the command module initialisation, add the following:

```cs
commands.RegisterCommands<MyCommands>();
```

This will enable all the commands in your command module.

The class should now look like this:

```cs
using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace MyFirstBot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "<your token here>",
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().StartsWith("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = ";;"
            });

            commands.RegisterCommands<MyCommands>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
```

## 4. Creating your first command

So now that you have your module, you want to add some commands to it. Let's add a first one. 

But before you do, let's explain a couple concepts. 

What are commands? How do they work? How do I make the library recognize something as a command?

Commands are basically methods with specific signatures. All commands must be public instance methods, that return a `Task`. 
They also need to take `CommandContext` as first argument. 

Commands work by invoking the method which is tied to the command when any users sends a message that consists of a prefix, 
command name, and its arguments, for example: `!hi`.

Commands are marked with a special attribute. When you register commands, the library looks for methods with that attribute 
and marks these methods as commands.

Armed with that knowledge, let's create your first command, a simple "hi, user!".

In the class, create a public async method, that returns a Task, and call it Hi. Make CommandContext its first argument. It 
should look like this:

```cs
public async Task Hi(CommandContext ctx)
{

}
```

Now, put the following code inside that method: `await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");`

You're not ready yet. Above the method, put the Command attribute. It should look like this: `[Command("hi")]`

Put together, the class should now look like this:

```cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MyFirstBot
{
    public class MyCommands
    {
        [Command("hi")]
        public async Task Hi(CommandContext ctx)
        {
            await ctx.RespondAsync($"👋 Hi, {ctx.User.Mention}!");
        }
    }
}
```

What this command will do is posting a message that contains the :wave: emoji, and says Hi, followed by a mention of the 
user who invoked the command.

Once this is all done, hit F5, and notice that console will light up with notifications. Go to Discord, and type `;;hi`. 
Your bot should now respond. If it did, congratulations!

![Step 1](/images/03_01_hi.png)

## 5. Using arguments

Close your bot, sit down, and listed to me.

CommandsNext is capable of automatically converting user-supplied data to a variety of types. The default argument converters 
can convert to the following:

* Integral types: `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`, `ulong`, `long`
* Floating-point types: `float`, `double`, `decimal`
* Text and character types: `string`, `char`
* Boolean types: `bool`
* Date and time types: `DateTime`, `DateTimeOffset`, `TimeSpan`
* Discord entities: `DiscordGuild`, `DiscordChannel`, `DiscordMember`, `DiscordUser`, `DiscordRole`, `DiscordMessage`, `DiscordEmoji`, `DiscordColor`

Using these is as simple as declaring additional arguments for your command function. Let's say you want to create a command 
that generates a random number between the two specified numbers. You can do it by adding two `int` arguments to your function.

For example:

```cs
[Command("random")]
public async Task Random(CommandContext ctx, int min, int max)
{
	var rnd = new Random();
	await ctx.RespondAsync($"🎲 Your random number is: {rnd.Next(min, max)}");
}
```

Now, if you hit F5, and go to your server, you can call `;;random 0 10`, and it will respond with a random number between 0 
and 10 exclusive.

![Step 2](/images/03_02_random.png)

## 6. Advanced subjects

Commands are covered more in-depth in [Emzi0767's Example bot #2](https://github.com/Emzi0767/DSharpPlus-Example-Bot/tree/master/DSPlus.Examples.CSharp.Ex02 "Example Bot #2"). 
If you want to check out all the cool things CommandsNext can do to make your life easier, make sure to check it out.
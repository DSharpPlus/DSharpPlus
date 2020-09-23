---
uid: commands_intro
title: CommandsNext Introduction
---

 >[!NOTE] 
 > This article assumes you've recently read the article on *[writing your first bot](xref:basics_first_bot)*.

# Introduction to CommandsNext 
This article will introduce basic concepts for the native command framework: *CommandsNext*.<br/>
This framework will, among other things, properly handle the registration and execution of your commands.

Be sure to install the `DSharpPlus.CommandsNext` package from NuGet before continuing.

![CommandsNext NuGet Package](/images/commands_intro_01.png)


<br/>
# Writing a Basic Command

## Create a Command Module
A command module is simply a class which acts as a container for your command methods. Instead of registering individual commands, 
you'd register a single command module which contains multiple commands. There's no limit to the amount of modules you can have,
and no limit to the amount of commands each module can contain. For example: you could have a module for moderation commands and 
a separate module for image commands. This will help you keep your commands organized and reduce the clutter in your project.

This demonstration, however, will be simple; consisting of one command module with a simple command.<br/>
We'll start by creating a new folder named `Commands` which contains a new class named `MyFirstModule`.

![Solution Explorer](/images/commands_intro_02.png)

Then mark the class as `public` and have it inherit from `BaseCommandModule`.
```cs
public class MyFirstModule : BaseCommandModule
{

}
```
This'll be necessary for each command module you create.


## Create a Command Method
Within our new module, create a method named `GreetCommand` that is marked as `async`, returns type `Task`, 
and takes one parameter of type `CommandContext`. In the example below, we've named that parameter `ctx`.
```cs
public async Task GreetCommand(CommandContext ctx)
{
    
}
```

In the body of this method, we'll use `CommandContext#RespondAsync` to send a simple message.
```cs
await ctx.RespondAsync("Greetings! Thank you for executing me!");
```

Finally, mark your command method with the `Command` attribute so CommandsNext will know to treat our method as a command method. 
This attribute takes a single parameter: the name of the command. 

We'll name our command *greet* to match the name of the method.
```cs
[Command("greet")]
public async Task GreetCommand(CommandContext ctx)
{
    await ctx.RespondAsync("Greetings! Thank you for executing me!");
}
```

<br/>
Your command module should now resemble this:
```cs
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

public class MyFirstModule : BaseCommandModule
{
    [Command("greet")]
    public async Task GreetCommand(CommandContext ctx)
    {
        await ctx.RespondAsync("Greetings! Thank you for executing me!");
    }
}
```

## Cleanup and Configuration 
Before we can run our new command, we'll need modify our main method.<br/>
Start by removing the event handler we created [previously](xref:basics_first_bot#spicing-up-your-bot).
```cs
var discord = new DiscordClient();

discord.MessageCreated += async (s, e) =>               // REMOVE
{                                                       // ALL
    if (e.Message.Content.ToLower().StartsWith("ping")) // OF
        await e.Message.RespondAsync("pong!");          // THESE
};                                                      // LINES

await discord.ConnectAsync();            
```

<br/>
Next, CommandsNext will need to be enabled on our `DiscordClient`.

You'll need to call the `UseCommandsNext` extension method on `DiscordClient`.<br/>
A `CommandsNextConfiguration` instance will need to be passed to that method.

Assign the result of `DiscordClient#UseCommandsNext` to a new varible named `commands`.

```cs
var discord = new DiscordClient();

var commands = discord.UseCommandsNext(new CommandsNextConfiguration());
```

Then, create an object initializer for `CommandsNextConfiguration`.<br/>
Assign the `StringPrefixes` property a new `string` array containing `!`.
```cs
new CommandsNextConfiguration()
{ 
    StringPrefixes = new[] { "!" }
}
```
This property will define the prefixes which will trigger our commands; in this case, it's only `!`

<br/>
Next, we'll register our command module.
Call the `RegisterCommands` method on our `CommandsNextExtension` instance and provide it with your command module.
```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext();

commands.RegisterCommands<MyFirstModule>();

await discord.ConnectAsync();            
```
Alternatively, you can pass in your assembly to register commands from all modules in your program.
```cs
commands.RegisterCommands(Assembly.GetExecutingAssembly());
```

<br/>
Your main method should look similar to the following:
```cs
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using MyFirstBot.Commands;

internal static async Task MainAsync()
{
    var discord = new DiscordClient(new DiscordConfiguration());
    var commands = discord.UseCommandsNext(new CommandsNextConfiguration());

    commands.RegisterCommands<MyFirstModule>();

    await discord.ConnectAsync();            
    await Task.Delay(-1);
}
```


## Running Your Command
It's now the moment of truth; all your blood, sweat, and tears have lead to this moment.
Hit `F5` on your keyboard to compile and run your bot, then execute your command in any channel that your bot account has access to.

![Congratulations, You've Won!](/images/commands_intro_03.png)

[That was easy](https://www.youtube.com/watch?v=GsQXadrmhws).


<br/>
# Taking User Input

## Command Arguments
Now that we have a basic command down, let's spice it up a bit by defining *arguments* to accept user input.

Defining an argument is simple; just add additional parameters to your signature of your command method. 
CommandsNext will automatically parse user input and populate the parameters of your command method with those arguments. 
To demonstrate, we'll modify our *greet* command to greet a user with a given name.

Head back to `MyFirstModule` and add a parameter of type `string` to the `GreetCommand` method.
```cs
[Command("greet")]
public async Task GreetCommand(CommandContext ctx, string name)
```
CommandsNext will now interpret this as a command named *greet* that takes one argument.

Next, replace our original response message with an [interpolated string](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)
which uses our new parameter.
```cs
public async Task GreetCommand(CommandContext ctx, string name)
{
    await ctx.RespondAsync($"Greetings, {name}! You're pretty neat!");
}
```

That's all there is to it. Smack `F5` and test it out in a channel your bot account has access to.

![Greet Part 2: Electric Boogaloo](/images/commands_intro_04.png)

<br/>
Now, you may have noticed that providing more than one word simply does not work.<br/>
For example, `!greet Luke Smith` will result in no response from your bot.

This fails because CommandsNext cannot find a valid overload for your command.

CommandsNext will split arguments by whitespace. This means `Luke Smith` is counted as two separate arguments; `Luke` and `Smith`.
In addition to this, CommandsNext will attempt to find and execute an overload of your command that has the *same number* of provided arguments.
Together, this means that any additional arguments will prevent CommandsNext from finding a valid overload to execute.

Yes, this means CommandsNext supports overloads; we'll touch on that a bit further down in this article.</br>
For now though, we'll take a look at a few different ways to work with the above.

<br/>
The simplest way to go about this would be to wrap your input with double quotes.<br/>
CommandsNext will parse this as one argument, allowing your command to be executed.
```
!greet "Luke Smith"
```
This option is generally not desired by the end user.

A more obvious solution is adding additional parameters to the method signature of your command.<br/>
```cs
public async Task GreetCommand(CommandContext ctx, string firstName, string lastName)
```
Do keep in mind that providing fewer arguments will put yourself in the same situation.<br/>
Consider assigning default values to non-vital parameters, the `lastName` parameter being a good example.

Lastly, you can use the `RemainingText` attribute on your parameter.<br/>
This attribute will instruct CommandsNext to parse all remaining arguments into that parameter. 
```cs
public async Task GreetCommand(CommandContext ctx, [RemainingText]string name)
```

<br/>
Each of these has their own caveats; it'll be up to you to choose the best solution for your commands.


## Argument Converters
CommandsNext can convert arguments, which are natively `string`, to the type specified by a command method parameter.
This functionality is powered by *argument converters*, and it'll help to eliminate the boilerplate code needed to parse and convert `string` arguments. 

CommandsNext has built-in argument converters for the following types:

Category|Types
:---:|:---
Discord|`DiscordGuild`, `DiscordChannel`, `DiscordMember`, `DiscordUser`,<br/>`DiscordRole`, `DiscordMessage`, `DiscordEmoji`, `DiscordColor`
Integral|`byte`, `short`, `int`, `long`, `sbyte`, `ushort`, `uint`, `ulong`
Floating-Point|`float`, `double`, `decimal`
Date|`DateTime`, `DateTimeOffset`, `TimeSpan`
Character|`string`, `char`
Boolean|`bool`

If all those types aren't enough, you're also able to provide your own [custom argument converters](xref:commands_argument_converters).


<br/>
Let's do a quick demonstration of the built-in converters. 

Open `MyFirstModule` one more time and head over to our `GreetCommand` method.
Change the name of the `name` parameter to `member`, and change its type from `string` to `DiscordMember`.
```cs
public async Task GreetCommand(CommandContext ctx, DiscordMember member)
```
Instead of accepting a plain string, our command is now able to accept mentions, usernames, nicknames, and user IDs.
Our response will need to be modified because of this change.
We'll make use of the `Mention` property on `DiscordMember` to mention the provided guild member.
```cs
public async Task GreetCommand(CommandContext ctx, DiscordMember member)
{
    await ctx.RespondAsync($"Greetings, {member.Mention}! Enjoy the mention!");
}
```

Now, run your bot once more with `F5` and give this a try in a text channel.

![According to all known laws of aviation,](/images/commands_intro_05.png)

![there is no way a bee should be able to fly.](/images/commands_intro_06.png)

![Its wings are too small to get its fat little body off the ground.](/images/commands_intro_07.png)

Super simple, just as before.


# Command Overloads
As of version 4.0.0, CommandsNext has support for overloading command methods.


# Further Reading
Now that you have a basic understanding of CommandsNext, you should check out [PLACEHOLDER]() and follow it up with [PLACEHOLDER]().

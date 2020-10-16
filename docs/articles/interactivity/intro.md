# Spicing your commands up with Interactivity module

Can bots feel? Well today you are going to find out.

## 1. Installing Interactivity

Using the procedures in the first bot article, install a NuGet package called `DSharpPlus.Interactivity`.

Now you need to enable Interactivity module on your DiscordClient. Add a new field to your bot's `Program` class: 
`static InteractivityModule interactivity;`

Visual Studio will complain, you also need to add `using DSharpPlus.Interactivity;` to your usings in both the command module 
and the bot class.

Before you connect, enable the module on your client: 

```cs
interactivity = discord.UseInteractivity();
```

This will enable the module.

## 2. Spicing up that `hi` command

Go back to the `hi` command. How about asking the bot how does it feel?

Interactivity allows you to wait for variety of user-triggered events, such as messages, reactions, or typing indicators. With 
this, you can make the bot wait for a specific message.

This is what you're going to do. Below the respond code, add the following:

```cs
var interactivity = ctx.Client.GetInteractivityModule();
var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && xm.Content.ToLower() == "how are you?", TimeSpan.FromMinutes(1));
if (msg.Result != null)
	await ctx.RespondAsync("I'm fine, thank you!");
```

Let's quickly dissect the code.

First, it gets the interactivity module from your client.

Next, it waits for a message that matches a predicate. In this case, the predicate waits for a message that was sent by the 
user who invoked the command, and says "how are you?". The wait has a limit of 1 minute. After that, the method returns.

Finally, it checks if a message was found. If it was, it's going to be non-null, and it can thank the user for concern!

Start the bot, and try invoking `;;hi`. After the bot responds, say `how are you?`. Then try again and don't say that. Notice 
the difference?

![Step 1](/images/04_01_hi_how_are_you.png "Hi! How are you?")

## 3. Advanced subjects

Interactivity is covered more in-depth in [Emzi0767's Example bot #3](https://github.com/Emzi0767/DSharpPlus-Example-Bot/tree/master/DSPlus.Examples.CSharp.Ex03 "Example Bot #3"). If you want to check out all the cool things Interactivity can 
do, make sure to check it out.

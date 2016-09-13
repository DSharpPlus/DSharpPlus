# Creating Commands #
First, you'll need to initialize your bot. This is a rather simple process:

`DiscordClient client = new DiscordClient("bot_token", true);`

Once you've done this, you can begin adding commands.
This can be done in one of two ways:

####In-line Decleration####
----
Commands can be declared simply by running AddCommand with the Create function. This is generally for commands that have simple functionality, and don't require many extra tidbits to perform their logic.

`client.AddCommand(DiscordCommand.Create("greet"));`
So now we have our command, however it doesn't seem to have any logic. _Let's add some._

```client.AddCommand(DiscordCommand.Create("greet")
	.Do(async e =>
	{
	await e.Channel.SendMessageAsync("Hello!");
	})
);```
```
Alright, so now we have a command that will send the message "Hello!" to whoever runs the command. But what if we'd like for it to mention the user as well?
```
    client.AddCommand(DiscordCommand.Create("greet")
    .Do(async e =>
    {
    await e.Channel.SendMessageAsync("Hello, " + e.Mention + "!");
})
);```
So, we've made a simple ``!greet`` command, which will make our bot greet whoever sends the command. But what if we'd like ``!hello`` to have the same functionality? Well that's simple, just add .Alias("hello") to the command, like so:

    client.AddCommand(DiscordCommand.Create("greet")
    .Do(async e =>
    {
	    await e.Channel.SendMessageAsync("Hello, " + e.Member.Mention + "!");
	})
	.Alias("hello")
	);
Welp, that's the basics of how to create a bot using ``DiscordCommand.Create``, AKA inline declaration.

####Class Declaration####
----
So what if you have some commands that require quite a bit of code to run, and you'd rather not fill your main function with a ton of extra, unrelated code?
That's where class declaration comes in. 
The first step is to create a class that extends **DiscordCommand**.

    class Command_Greet : DiscordCommand
Now, we can add all of our logic within our own functions, without having to fill up our main class. Here's an example, using the !greet command from above.

    class Command_Greet : DiscordCommand
    {
        public Command_Greet() : base()
        {
            Keyword = "greet";
            Aliases.Add("hello");
            SetInvokeFunction(new Action<DiscordCommandEventArgs>(RunCommand));
        }

        public void RunCommand(DiscordCommandEventArgs e)
        {
            e.Channel.SendMessage("Hello, " + e.Member.Mention + "!");
        }
    }
So now, we need to add our command to our bot. This can be done like so:

    DiscordClient client = new DiscordClient("bot_token", true);
    client.AddCommand(new Command_Greet());
and that's it! We've successfully made a command in both the simple way, and more-complicated-but-also-cooler way. :)

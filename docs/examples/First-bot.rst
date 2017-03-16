Making your first bot
=======================

Making your first bot in C# using DSharpPlus library is an easy task, however it does require good knowledge of C#.

If you do not know C# well enough, I recommend checking `this video series <https://channel9.msdn.com/Series/C-Fundamentals-for-Absolute-Beginners>`_ out.

Step 1: Create the application
--------------------------------

We begin the adventure by creating an application and bot user for your bot.

1. Go to `my applications <https://discordapp.com/developers/applications/me>`_ page on Discord Developer portal.
2. Press the **new app** button.

.. image:: http://i.imgur.com/IVsPyNw.png

3. **New app** page will open. Enter your bot's name in the **app name** field (1), and its description in the **description** field (2).

.. image:: http://i.imgur.com/3mrEG9x.png

	* You can optionally give it an avatar by pressing on the **avatar** button (3).

4. When you're done, press the **create app** button.

.. image:: http://i.imgur.com/ur3HFng.png

5. When the app is created, press the **create bot user** button.

.. image:: http://i.imgur.com/b69CHy7.png

6. Once this is done, you will need to copy the **bot's token**. Under **app bot user**, there's a **token** field, press **click to reveal** and copy **the resulting value**.

.. image:: http://i.imgur.com/00b4Nt8.png

7. Save the token somewhere safe. You will need it later.

Step 2: Setting up your project
----------------------------------

Ok. So we have a bot application created, but it does nothing. Let's change this.

1. Open your IDE of choice. In here, we'll be using Visual Studio 2017.
2. Create a new project. Make sure it's a **Console App (.NET Framework)** project. Name it whatever you want.

.. image:: http://i.imgur.com/OSsP7mE.png
.. image:: http://i.imgur.com/AfTQimU.png

3. On the right side, there should be **Solution Explorer** pane. Right-click your project, and select **Manage NuGet packages**.

.. image:: http://i.imgur.com/AycWl6p.png
.. image:: http://i.imgur.com/3lPMSWW.png

4. A new tab will open in your project. If it's not selected, click on **Browser** (1), check **Include prerelase** (2), and search for ``dsharpplus`` (3).

.. image:: http://i.imgur.com/uIGz4MB.png

5. Select the result, and press **Install**.

.. image:: http://i.imgur.com/bjW4Ant.png

6. Now, once this is done, go to **Installed** (1), clear the search box (2), select ``Baseclass.Contrib.Nuget.Output``, and on the right, select ``2.2.0-xbuild02`` version (3), and press **Update** (4).

.. image:: http://i.imgur.com/bWJifiz.png

	* *Note*: this is optional if these packages are already at these or newer versions.

7. Repeat this ``libsodium-net``, but install version ``0.10.0``.
8. Now, our project is ready to begin. Close the NuGet tab, and go back to your project.

Step 3: Adding the bot to your server
---------------------------------------

1. Go back to your app page, and copy your bot's **client ID**.

.. image:: http://i.imgur.com/NuAPpoY.png

2. Go to ``https://discordapp.com/oauth2/authorize?client_id=your_app_id_here&scope=bot&permissions=0``. Replace ``your_app_id_here`` with the **client ID** you copied.
3. On the page, select **your server** (1), and press **authorize** (2).

.. image:: http://i.imgur.com/QeH0o5S.png

4. Done! You can now run the bot!

.. image:: http://i.imgur.com/LF1gpm2.png

Step 4: Basic bot, connecting to Discord
------------------------------------------

Now that our project is set up, we can begin coding. In the [generated file](http://i.imgur.com/94FPUA0.png), you will quickly notice a couple things:

1. ``using`` section.
2. ``Main`` method.

Here's what we need to do:

1. Add `using DSharpPlus to the 1st section.
2. Add a new method, ``public static async Task Run()`` under the ``Main`` method.
3. Add the following code to 2nd section: ``Run().GetAwaiter().GetResult()``
4. Add the following code to the ``Run`` method: ::

	var discord = new DiscordClient(new DiscordConfig
	{
		AutoReconnect = true,
		DiscordBranch = Branch.Stable,
		LargeThreshold = 250,
		LogLevel = LogLevel.Unnecessary,
		Token = "insert your token here",
		TokenType = TokenType.Bot,
		UseInternalLogHandler = false
	});

	await discord.Connect();

	await Task.Delay(-1);

5. Replace ``insert your token here`` with the token you saved in Step 1.
6. Press **Start**.

.. image:: http://i.imgur.com/VkclYlr.png

7. Congratulations. Your bot is now running, although it really does nothing. You should see it come online.

Step 5: Events
----------------

We have connected our bot and added it to a server. But it does nothing (yet). Let's change that. Close your bot, and go back to Visual Studio.

*Note*: All of the below will be occuring between the declaration of ````discord```` and calling ``await discord.Connect()``, in the ``Run`` method.

1. First, we might want to light our console up with some messages from the bot. Let's add a handler for this: ::

	discord.DebugLogger.LogMessageReceived += (o, e) =>
	{
		Console.WriteLine($"[{e.TimeStamp}] [{e.Application}] [{e.Level}] {e.Message}");
	};

2. Next, we might want to let ourselves know when do guilds become available. Remember, discord doesn't send you all the guilds at once, it sends them one-by-one. To achieve this objective, we need to hook the ``GuildAvailable`` event: ::

	discord.GuildAvailable += e =>
	{
		discord.DebugLogger.LogMessage(LogLevel.Info, "discord bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
		return Task.Delay(0);
	};
   
3. Probably the most important objective, let's make our bot respond to messages. This is done by hooking the ``MessageCreated`` event: ::

	discord.MessageCreated += async e =>
	{
		if (e.Message.Content.ToLower() == "ping")
			await e.Message.Respond("pong");
	};
   
4. Run your bot. When it comes online, type ``ping`` in chat. If your bot responds with ``pong``, congratulations, you did well.

Step 6: Commands
------------------

Well, this is cool, but handing commands like this might get tedious real fast. The solution? Command module. Close the bot and let's go back to Visual Studio.

Unfortunately, for this you will need to clone the repository and build the module yourself, because as of this writing, the module is not yet on NuGet. Once you have the module built and referenced, let's make the necessary changes to our code.

1. Add ``using DSharpPlus.Commands;`` to the ``using`` section.
2. In your ``Run`` method, add the following code: ::
   
	discord.UseCommands(new CommandConfig
	{
		Prefix = "#",
		SelfBot = false
	});
   
3. Now we have a command service set up, but no commands yet. Let's change that. We're going to create a ``hello`` command. We do that as follows: ::
   
	discord.AddCommand("hello", async e =>
	{
		await e.Message.Respond($"Hello, {e.Message.Author.Mention}!");
	});
   
4. Now let's run our bot. Once it comes online, say ``#hello``. The bot should respond by saying ``Hello, @yourname!``. If it did, good job. From here, you can do other things.

Summary
=========

You now have a bot that responds to messages and commands. You can extend it further using what you just learned, and reading the documentation.

By now, your code should look like this: ::

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using DSharpPlus;
	using DSharpPlus.Commands;

	namespace DspBot
	{
		class Program
		{
			static void Main(string[] args)
			{
				Run().GetAwaiter().GetResult();
			}

			public static async Task Run()
			{
				var discord = new DiscordClient(new DiscordConfig
				{
					AutoReconnect = true,
					DiscordBranch = Branch.Stable,
					LargeThreshold = 250,
					LogLevel = LogLevel.Unnecessary,
					Token = "insert your token here",
					TokenType = TokenType.Bot,
					UseInternalLogHandler = false
				});

				discord.DebugLogger.LogMessageReceived += (o, e) =>
				{
					Console.WriteLine($"[{e.TimeStamp}] [{e.Application}] [{e.Level}] {e.Message}");
				};

				discord.GuildAvailable += e =>
				{
					discord.DebugLogger.LogMessage(LogLevel.Info, "discord bot", $"Guild available: {e.Guild.Name}", DateTime.Now);
					return Task.Delay(0);
				};

				discord.MessageCreated += async e =>
				{
					if (e.Message.Content.ToLower() == "ping")
						await e.Message.Respond("pong");
				};

				discord.UseCommands(new CommandConfig
				{
					Prefix = "#",
					SelfBot = false
				});

				discord.AddCommand("hello", async e =>
				{
					await e.Message.Respond($"Hello, {e.Message.Author.Mention}!");
				});

				await discord.Connect();

				await Task.Delay(-1);
			}
		}
	}

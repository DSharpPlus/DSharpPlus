# Getting started with DSharpPlus - your first DSharpPlus bot

You're still with me - good! You have created an application and a user for your bot. Now we will focus on bringing it to life.

In order for that to happen, you need to make a program that connects to discord as your bot.

This guide requires you to have solid basics in C#. Open your IDE. If you don't have one installed, I recommend installing 
one of the following:

* [Visual Studio 2017 Community](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15 "Visual Studio 2017 Community")
* [Visual Studio Code](https://code.visualstudio.com/download "Visual Studio Code")

When installing Visual Studio 2017, make sure you select the **.NET Core cross-platform development** option.

![.NET Core in Visual Studio 2017](/images/02_01_vs_netcore.png ".NET Core in Visual Studio 2017")

This tutorial will be presented using Visual Studio 2017, however the instructions should still apply to VS Code and other .NET 
IDEs (so long as they support .NET Core).

## Note for Windows 7 users

If you're using Windows 7, create a regular .NET Framework project (it's required you target at least .NET 4.5; we recommend 
4.7).

Make sure you read the article about [Alternate WebSocket Client Implementations](/articles/getting_started/alternate_ws.html).

## 1. Creating your project

To start, you need to create a new project. Click the **Create new project...** button, and select **Console App (.NET Core)** 
as your project template. Give it some name, in here it's **MyFirstBot**.

![Step 1](/images/02_02_new_project.png "New project")

![Step 2](/images/02_03_new_project_settings.png ".NET Core App")

## 2. Adding the NuGet package

Once you create your project, you will be presented with a blank project template, that only prints Hello World! to the console. 
Before you embark on your journey, you need to add DSharpPlus package reference. Locate the Solution Explorer on the right 
side of the screen. 

![Step 3](/images/02_04_solution_explorer.png "Finding the solution explorer")

Right-click the project (as highlighted on the above picture), and select **Manage NuGet Packages...**.

![Step 4](/images/02_05_manage_packages.png "Manage Packages")

NuGet package manager tab will open. In it, press **Browse**, and in the **Search** field, type `DSharpPlus`.

![Step 5](/images/02_06_nuget.png "NuGet interface")

Once this is done, select the **DSharpPlus** package, and on the right pane ensure that version selected is **Latest stable**, 
then press **Install**.

![Step 6](/images/02_07_installing.png "Installing the DSharpPlus package")

## 3. Making your first bot

Now that your project has the required DSharpPlus components, let's move on to actually making the bot.

The first thing you will want to do is adding a using section for the library's components in the code. You can do that 
by adding `using DSharpPlus;` under `using System;`. Once that is done, locate the `Main` method, and delete all of its 
contents. Create a new method below it, with the following signature:

```cs
static async Task MainAsync(string[] args)
{

}
```

Visual Studio will complain. Hover over `Task`, and apply the recommended solution. It should add 
`using System.Threading.Tasks;` to your usings section.

Then, go back to `Main`, and place the following code in it: `MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();`

Now, above the `Main` method, create a static field for your Discord client: `static DiscordClient discord;`. This field will 
hold your DiscordClient instance, which you will be using to interact with Discord API.

Now go back inside `MainAsync` method. You need to initialize the client. Remeber that token I told you to take note of? 
Copy it, you are about to use it.

Initialize the Discord client: 

```cs
discord = new DiscordClient(new DiscordConfiguration
{
	Token = "<paste the token here>",
	TokenType = TokenType.Bot
});
```

You have initialized your client instance, but it does nothing yet. Let's make it listen for incoming messages, and respond 
with "pong" to messages that start with "ping". For that, you need to utilize the @DSharpPlus.DiscordClient.MessageCreated event.
of the client. Let's hook it then:

```cs
discord.MessageCreated += async e =>
{
	if (e.Message.Content.ToLower().StartsWith("ping"))
		await e.Message.RespondAsync("pong!");
};
```

Now, once the bot connects, it will respond with "pong!" to each message that starts with "ping".

But that will happen only once you connect, so how do you do that? You use the @DSharpPlus.DiscordClient.ConnectAsync method.
of the client. You will need to `await` it, which is why you had to make an asynchronous `Main` method.

```cs
await discord.ConnectAsync();
```

This is not everything yet. If you start the bot now, it will just flash and quit. To prevent that, you need to add an 
infinite wait at the end of your `MainAsync` method:

```cs
await Task.Delay(-1);
```

## 4. Putting it all together

Once you are done, your code will look more or less like this:

```cs
using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace MyFirstBot
{
    class Program
    {
        static DiscordClient discord;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = "<your token here>",
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

If this is the case, you are ready to move on.

## 5. Testing

Hit F5. This will compile your code and run the project. If all went well, your bot should now be online and respond to 
messages that start with ping.

If you're a Windows 7 user, it won't work. Read the [Alternate WebSocket Client Implementations](/articles/getting_started/alternate_ws.html) 
on ways to fix the issue.

![Step 9](/images/02_08_alive.png)

If it works, congratulations! You can now try some of the more advanced subjects from the list on the left.
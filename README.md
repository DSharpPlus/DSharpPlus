# DSharpPlus.SlashCommands

[![Nuget](https://img.shields.io/nuget/vpre/IDoEverything.DSharpPlus.SlashCommands?style=flat-square)](https://www.nuget.org/packages/IDoEverything.DSharpPlus.SlashCommands)
[![Discord](https://img.shields.io/discord/801857343930761281?label=Discord&logo=Discord&style=flat-square)](https://discord.gg/2ZhXXVJYhU)

An extension for [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) to make slash commands easier.

Join the [Discord server](https://discord.gg/2ZhXXVJYhU) for any questions, help or discussion.

# Documentation

DSharpPlus doesn't currently have a slash command framework. You can use this library to somewhat get an idea of how you can implement slash commands into your bot.

I have done my best to make this as similar to CommandsNext as possible to make it a smooth experience. However, there are some limitations in comparison. The library does not support:

    Registering or editing commands at runtime
    Sharding
    Any pre execution checks
   
While you can make commands at runtime, if you have a command class registered for that guild/globally if you're making global commands, it will be overwritten (therefore probably deleted) on the next startup due to the limitations of the bulk overwrite endpoint.

Now, on to the actual guide:
## Installing
Package-Manager: `Install-Package IDoEverything.DSharpPlus.SlashCommands`

.NET CLI: `dotnet add package IDoEverything.DSharpPlus.SlashCommands`

## Important: Authorizing your bot

For a bot to make slash commands in a server, it must be authorized with the applications.commands scope as well. In the OAuth2 section of the developer portal, you can check the applications.commands box to generate an invite link. You can check the bot box as well to generate a link that authorizes both. If a bot is already authorized with the bot scope, you can still authorize with just the applications.commands scope without having to kick out the bot.

If your bot isn't properly authorized, a 403 exception will be thrown on startup.

# Setup

Add the using reference to your bot class:
```cs
using DSharpPlus.SlashCommands;
```

You can then register a `SlashCommandsExtension` on your `DiscordClient`, similar to how you register a `CommandsNextExtension`

```cs
var slash = discord.UseSlashCommands();
```

## Making a command class
Similar to CommandsNext, you can make a module for slash commands and make it inherit from `SlashCommandModule`
```cs
public class SlashCommands : SlashCommandModule
{
  //commands
}
```
You have to then register it with your `SlashCommandsExtension`.

Slash commands can be registered either globally or for a certain guild. However, if you try to register them globally, they can take up to an hour to cache across all guilds. So, it is recommended that you only register them for a certain guild for testing, and only register them globally once they're ready to be used.

To register your command class,
```cs
//To register them for a single server, recommended for testing
slash.RegisterCommands<SlashCommands>(guild_id);

//To register them globally, once you're confident that they're ready to be used by everyone
slash.RegisterCommands<SlashCommands>();
```
*Make sure that you register them before your `ConnectAsync`*

## Making Commands!
On to the exciting part. 

Slash command methods must be `Task`s and have the `SlashCommand` attribute. The first argument for the method must be an `InteractionContext`. Let's make a simple slash command:
```cs
public class SlashCommands : SlashCommandModule
{
    [SlashCommand("test", "A slash command made to test the DSharpPlusSlashCommands library!")]
    public async Task TestCommand(InteractionContext ctx)
    {
    
    }
}
```

To make a response, you must run `CreateResponseAsync` on your `InteractionContext`. `CreateResponseAsync` takes two arguments. The first is a [`InteractionResponseType`](https://dsharpplus.github.io/api/DSharpPlus.InteractionResponseType.html):
* `DeferredChannelMessageWithSource` - Acknowledges the interaction, doesn't require any content.
* `ChannelMessageWithSource` - Sends a message to the channel, requires you to specify some data to send.

An interaction expires in 3 seconds unless you make a response. If the code you execute before making a response has the potential to take more than 3 seconds, you should first create a `DeferredChannelMessageWithSource` response, and then edit it after your code executes.

The second argument is a type of [`DiscordInteractionResponseBuilder`](https://dsharpplus.github.io/api/DSharpPlus.Entities.DiscordInteractionResponseBuilder.html). It functions similarly to the DiscordMessageBuilder, except you cannot send files, and you can have multiple embeds.

A simple response would be like:
```cs
[SlashCommand("test", "A slash command made to test the DSharpPlusSlashCommands library!")]
public async Task TestCommand(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success!"));
}
```
If your code will take some time to execute:
```cs
[SlashCommand("test", "A slash command made to test the DSharpPlusSlashCommands library!")]
public async Task TestCommand(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
    await Task.Delay(5000);
    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("5 second delay complete!"));
}
```
You can also override `BeforeExecutionAsync` and `AfterExecutionAsync` to run code before and after all the commands in a module. This does not apply to groups, you have the override them individually for the group's class.
### Arguments
If you want the user to be able to give more data to the command, you can add some arguments.

Arguments must have the `Option` attribute, and can only be of type `string`, `long`, `bool`, `DiscordUser`, `DiscordChannel`, `DiscordRole` and `Enum`. If you want to make them optional, you can assign a default value as well.

You can also predefine some choices for the option. Choices only work for `string` and `long` arguments. THere are several ways to use them:
1. With the `Choice` attribute. You can add multiple attributes to add multiple choices.
2. You can also define choices using enums, see the example below.
3. You can use a `ChoiceProvider` to run code to get the choices from, see the example below.

(second and third method contributed by @Epictek)

Some examples:
```cs
[SlashCommand("avatar", "Get someone's avatar")]
public async Task Av(InteractionContext ctx, [Option("user", "The user to get it for")] DiscordUser user = null)
{
    user ??= ctx.Member;
    var embed = new DiscordEmbedBuilder
    {
        Title = $"Avatar",
        ImageUrl = user.AvatarUrl
    }.
    WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl).
    WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
}

//Attribute choices
[SlashCommand("phrase", "Sends a certain phrase in the chat!")]
public async Task Phrase(InteractionContext ctx,
    [Choice("phrase1", "all's well that ends well")]
    [Choice("phrase2", "be happy!")]
    [Option("phrase", "the phrase to respond with")] string phrase)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(phrase));
}

//Enum choices
enum MyEnum
{
    [ChoiceName("Option 1")]
    option1,
    [ChoiceName("Option 2")]
    option2,
    [ChoiceName("Option 3")]
    option3
}
    
[SlashCommand("enum", "Test enum")]
public async Task EnumCommand(InteractionContext ctx, MyEnum myEnum = MyEnum.option1)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(myEnum.GetName()));
}

//ChoiceProvider choices
public class TestChoiceProvider : IChoiceProvider
{
    public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        return new DiscordApplicationCommandOptionChoice[]
        {
            new DiscordApplicationCommandOptionChoice("testing", "testing"),
            new DiscordApplicationCommandOptionChoice("testing2", "test option 2")
        };
    }
}
        
[SlashCommand("choiceprovider", "test")]
public async Task ChoiceProviderCommand(InteractionContext ctx,
    [ChoiceProvider(typeof(TestChoiceProvider))]
    [Option("option", "option")] string option)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(option));
}
```
### Groups
You can have slash commands in groups. Their structure is explained [here](https://discord.com/developers/docs/interactions/slash-commands#nested-subcommands-and-groups), I would highly recommend reading it to understand how they work. To register groups you need a container which inherits from `SlashCommandModule`. Inside this container you can register groups of commands:
```cs
public class GroupContainer : SlashCommandModule 
{
    //For a group and subcommands inside the group
    [SlashCommandGroup("group", "description")]
    public class Group : SlashCommandModule
    {
        [SlashCommand("command", "description")]
        public async Task Command(InteractionContext ctx) {}
    
        [SlashCommand("command2", "description")]
        public async Task Command2(InteractionContext ctx) {}
    
        [SlashCommand("command3", "description")]
        public async Task Command3(InteractionContext ctx) {}
    }
  
    //For subgroups inside groups
    [SlashCommandGroup("group", "description")]
    public class Group : SlashCommandModule
    {
        [SlashCommandGroup("subgroup", "description")]
        public class SubGroup : SlashCommandModule
        {
            [SlashCommand("command", "description")]
            public async Task Command(InteractionContext ctx) {}
    
            [SlashCommand("command2", "description")]
            public async Task Command2(InteractionContext ctx) {}
    
            [SlashCommand("command3", "description")]
            public async Task Command3(InteractionContext ctx) {}
        }
    
        [SlashCommandGroup("subgroup2", "description")]
        public class SubGroup2 : SlashCommandModule
        {
            [SlashCommand("command", "description")]
            public async Task Command(InteractionContext ctx) {}
    
            [SlashCommand("command2", "description")]
            public async Task Command2(InteractionContext ctx) {}
    
            [SlashCommand("command3", "description")]
            public async Task Command3(InteractionContext ctx) {}
        }
    }
}
```
### Dependency Injection
To pass in a service collection, provide a `SlashCommandsConfiguration` in `UseSlashCommands`.
```cs
var slash = discord.UseSlashCommands(new SlashCommandsConfiguration
{
    Services = new ServiceCollection().AddSingleton<Random>().BuildServiceProvider()
});
```
(Thanks to @sssvt-drabek-stepan for adding this)

# Issues and contributing
If you find any issues or bugs, you should join the discord server and discuss it. If it's an actual bug, you can create an [issue](https://github.com/IDoEverything/DSharpPlus.SlashCommands/issues). If you would like to contribute or make changes, feel free to open a [pull request](https://github.com/IDoEverything/DSharpPlus.SlashCommands/pulls).

# Questions?
Join the [discord server](https://discord.gg/2ZhXXVJYhU)!

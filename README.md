# DSharpPlus.SlashCommands

[![Nuget](https://img.shields.io/nuget/vpre/IDoEverything.DSharpPlus.SlashCommands?style=flat-square)](https://www.nuget.org/packages/IDoEverything.DSharpPlus.SlashCommands)
[![Discord](https://img.shields.io/discord/801857343930761281?label=Discord&logo=Discord&style=flat-square)](https://discord.gg/2ZhXXVJYhU)

An extension for [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) to make slash commands easier.

Join the [Discord server](https://discord.gg/2ZhXXVJYhU) for any questions, help or discussion.

# Documentation

DSharpPlus doesn't currently have a slash command framework. You can use this library to implement slash commands into your bot.

I have done my best to make this as similar to CommandsNext as possible to make it a smooth experience. However, the library does not support registering or editing commands at runtime. While you can make commands at runtime using the methods on the client, if you have a command class registered for that guild/globally if you're making global commands, it will be overwritten (therefore probably deleted) on the next startup due to the limitations of the bulk overwrite endpoint.

Now, on to the actual guide:
## Installing
Simply search for `IDoEverything.DSharpPlus.SlashCommands` and install the latest version. If you're using command line:

Package-Manager: `Install-Package IDoEverything.DSharpPlus.SlashCommands`

.NET CLI: `dotnet add package IDoEverything.DSharpPlus.SlashCommands`

The current version of the library depends on the DSharpPlus nightly version. If you're using the stable nuget version, [update to the nightly version](https://dsharpplus.github.io/articles/misc/nightly_builds.html).
# Important: Authorizing your bot

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
Similar to CommandsNext, you can make a module for slash commands and make it inherit from `ApplicationCommandModule`
```cs
public class SlashCommands : ApplicationCommandModule
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
public class SlashCommands : BaseCommandModule
{
    [SlashCommand("test", "A slash command made to test the DSharpPlusSlashCommands library!")]
    public async Task TestCommand(InteractionContext ctx) { }
}
```

To make a response, you must run `CreateResponseAsync` on your `InteractionContext`. `CreateResponseAsync` takes two arguments. The first is a [`InteractionResponseType`](https://dsharpplus.github.io/api/DSharpPlus.InteractionResponseType.html):
* `DeferredChannelMessageWithSource` - Acknowledges the interaction, doesn't require any content.
* `ChannelMessageWithSource` - Sends a message to the channel, requires you to specify some data to send.

An interaction expires in 3 seconds unless you make a response. If the code you execute before making a response has the potential to take more than 3 seconds, you should first create a `DeferredChannelMessageWithSource` response, and then edit it after your code executes.

The second argument is a type of [`DiscordInteractionResponseBuilder`](https://dsharpplus.github.io/api/DSharpPlus.Entities.DiscordInteractionResponseBuilder.html). It functions similarly to the DiscordMessageBuilder, except you cannot send files, and you can have multiple embeds.

If you want to send a file, you'll have to edit the response.

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
[SlashCommand("delaytest", "A slash command made to test the DSharpPlusSlashCommands library!")]
public async Task DelayTestCommand(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
    
    //Some time consuming task like a database call or a complex operation
    
    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Thanks for waiting!"));
}
```
You can also override `BeforeExecutionAsync` and `AfterExecutionAsync` to run code before and after all the commands in a module. This does not apply to groups, you have the override them individually for the group's class.
`BeforeExecutionAsync` can also be used to prevent the command from running.

### Arguments
If you want the user to be able to give more data to the command, you can add some arguments.

Arguments must have the `Option` attribute, and can be of type:
* `string`
* `long` or `long?`
* `bool` or `bool?`
* `double` or `double?`
* `DiscordUser` - This can be cast to `DiscordMember` if the command is run in a guild 
* `DiscordChannel`
* `DiscordRole`
* `SnowflakeObject` - This can accept both a user and a role; you can cast it `DiscordUser`, `DiscordMember` or `DiscordRole` to get the actual object
* `Enum` - This can used for choices through an enum; read further
 
If you want to make them optional, you can assign a default value.

You can also predefine some choices for the option. Choices only work for `string`, `long` or `double` arguments. THere are several ways to use them:
1. Using the `Choice` attribute. You can add multiple attributes to add multiple choices.
2. You can define choices using enums. See the example below.
3. You can use a `ChoiceProvider` to run code to get the choices from a database or similar. See the example below.

(second and third method contributed by @Epictek)

Some examples:
```cs
//Attribute choices
[SlashCommand("ban", "Bans a user")]
public async Task Ban(InteractionContext ctx, [Option("user", "User to ban")] DiscordUser user,
    [Choice("None", 0)]
    [Choice("1 Day", 1)]
    [Choice("1 Week", 7)]
    [Option("deletedays", "Number of days of message history to delete")] long deleteDays = 0)
{
    await ctx.Guild.BanMemberAsync(user.Id, (int)deleteDays);
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Banned {user.Username}"));
}

//Enum choices
public enum MyEnum
{
    [ChoiceName("Option 1")]
    option1,
    [ChoiceName("Option 2")]
    option2,
    [ChoiceName("Option 3")]
    option3
}
    
[SlashCommand("enum", "Test enum")]
public async Task EnumCommand(InteractionContext ctx, [Option("enum", "enum option")]MyEnum myEnum = MyEnum.option1)
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
            //You would normally use a database call here
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

### Pre-execution checks
You can define some custom attributes that function as pre-execution checks, working very similarly to `CommandsNext`. Simply create an attribute that inherits `SlashCheckBaseAttribute` and override the methods.

There are also some built in ones, the same ones as on `CommandsNext` but prefix with `Slash` - for example the `SlashRequirePermissionsAttribute`
```cs
public class RequireUserIdAttribute : SlashCheckBaseAttribute
{
    public ulong UserId;
    
    public RequireUserIdAttribute(ulong userId)
    {
        this.UserId = userId;
    }

    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.User.Id == UserId) 
            return true;
        else
            return false;
    }
}

```
Then just apply it to your command
```cs
[SlashCommand("admin", "runs sneaky admin things")]
[RequireUserId(0000000000000)]
public async Task Admin(InteractionContext ctx) { //secrets }
```
To provide a custom error message when an execution check fails, hook the `SlashCommandErrored` event on your `SlashCommandsExtension`
```cs
SlashCommandsExtension slash = //assigned;
slash.SlashCommandErrored += async (s, e) =>
{
    if(e.Exception is SlashExecutionChecksFailedException slex)
    {
        foreach (var check in slex.FailedChecks)
          if (check is RequireUserIdAttribute att)
              await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Only <@{att.Id}> can run this command!"));
    }
};
```
Or to use a built in one:
```cs
[SlashCommand("ban", "Bans a user")]
[SlashRequirePermissions(Permissions.BanMembers)]
public async Task Ban(InteractionContext ctx, [Option("user", "User to ban")] DiscordUser user,
    [Choice("None", 0)]
    [Choice("1 Day", 1)]
    [Choice("1 Week", 7)]
    [Option("deletedays", "Number of days of message history to delete")] long deleteDays = 0)
{
    await ctx.Guild.BanMemberAsync(user.Id, (int)deleteDays);
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Banned {user.Username}"));
}
```

### Groups
You can have slash commands in groups. Their structure is explained [here](https://discord.com/developers/docs/interactions/slash-commands#nested-subcommands-and-groups). You can simply mark your command class with the `[SlashCommandGroup]` attribute.
```cs
//for regular groups
[SlashCommandGroup("group", "description")]
public class GroupContainer : ApplicationCommandModule 
{
    [SlashCommand("command", "description")]
    public async Task Command(InteractionContext ctx) {}
    
    [SlashCommand("command2", "description")]
    public async Task Command2(InteractionContext ctx) {}
}

//For subgroups inside groups
[SlashCommandGroup("group", "description")]
public class SubGroupContainer : ApplicationCommandModule
{
    [SlashCommandGroup("subgroup", "description")]
    public class SubGroup : ApplicationCommandModule
    {
        [SlashCommand("command", "description")]
        public async Task Command(InteractionContext ctx) {}
    
        [SlashCommand("command2", "description")]
        public async Task Command2(InteractionContext ctx) {}
    }
    
    [SlashCommandGroup("subgroup2", "description")]
    public class SubGroup2 : ApplicationCommandModule
    {
        [SlashCommand("command", "description")]
        public async Task Command(InteractionContext ctx) {}
    
        [SlashCommand("command2", "description")]
        public async Task Command2(InteractionContext ctx) {}
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
Property injection is implemented, however static properties will not be replaced. If you wish for a non-static property to be left alone, assign it the `DontInject` attribute. Property Injection can be used like so:
```cs
public class Commands : ApplicationCommandModule
{
    public Database Database { private get; set; } // The get accessor is optionally public, but the set accessor must be public.

    [SlashCommand("ping", "Checks the latency between the bot and it's database. Best used to see if the bot is lagging.")]
    public async Task Ping(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
    {
        Content = $"Pong! Database latency is {Database.GetPing()}ms."
    });
}
```
### Sharding
`UseSlashCommands` -> `UseSlashCommmandsAsync` which returns a dictionary.

You'll have to foreach over it to register events.

### Module Lifespans
You can specify a module's lifespan by applying the `SlashModuleLifespan` attribute on it. Modules are transient by default.

# Issues and contributing
If you find any issues or bugs, you should join the discord server and discuss it. If it's an actual bug, you can create an [issue](https://github.com/IDoEverything/DSharpPlus.SlashCommands/issues). If you would like to contribute or make changes, feel free to open a [pull request](https://github.com/IDoEverything/DSharpPlus.SlashCommands/pulls).

# Questions?
Join the [discord server](https://discord.gg/2ZhXXVJYhU)!

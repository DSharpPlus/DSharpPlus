---
uid: articles.commands.command_processors.text_commands.custom_prefix_handler
title: Custom Prefix Handler
---

# Adding dynamic prefixes

Prefixes are commonplace among Discord bots, and are used to determine if a message is a command or not. By default, DSharpPlus uses the `!` prefix, but you can change this to whatever you want. However, what if you want to have different prefixes for different servers? This is where custom prefix handlers come in.

There are two manners of going about this; you can either use a prefix resolver delegate, or implement the `IPrefixResolver` interface. The former is simpler, but the latter is more powerful.

## Prefix resolver delegate
The prefix resolver delegate is very simple to use. Any method that can be converted into a `Func<CommandsExtension, DiscordMessage, ValueTask<int>>` (a method that takes a `CommandsExtension` and a `DiscordMessage`, and returns a `ValueTask<int>`) can be used as a prefix resolver delegate. This method will be called for every message, and the return value will be used as the prefix length to slice off the prefix from the message content. If the return value is `-1`, the message will be ignored.

> [!IMPORTANT]
> Lambdas generated via reflection or compiled expressions should be compiled to `ResolvePrefixDelegateAsync`. Compiling to `Func<CommandsExtension, DiscordMessage, ValueTask<int>>` will result in a runtime exception.

The default prefix resolver uses the `!` prefix. To change this, you can use the following code:

```cs
DefaultPrefixResolver prefixResolver = new DefaultPrefixResolver(true, "!", "?");
TextCommandProcessor textCommandProcessor = new(new()
{
    // The default behavior is that the bot reacts to direct
    // mentions and to the "!" prefix. If you want to change
    // it, you first set if the bot should react to mentions
    // and then you can provide as many prefixes as you want.
    PrefixResolver = prefixResolver.ResolvePrefixAsync
});
```

In this example, the bot will react to bot mentions, to the `!` prefix, and the `?` prefix. The `true` argument in the `DefaultPrefixResolver` constructor specifies that the bot should treat it's own mention as a prefix: @BotName ping. This should usually be left to `true` as Discord will always pass the message content when the bot is mentioned, preventing the need to request for the message content privileged intent.

## IPrefixResolver

The `IPrefixResolver` interface has a bit more setup, but can make dynamic prefixes easier overall. The interface is as follows:

```cs
public interface IPrefixResolver
{
    ValueTask<int> ResolvePrefixAsync(CommandsExtension extension, DiscordMessage message);
}
```

On the surface, this is very similar to setting the delegate property, however implementing the interface allows you to participate in dependency injection.

A common scenario is using a database to retrieve a per-server prefix.
Here's an example of how you can implement the `IPrefixResolver` interface:

```cs
public class CustomPrefixResolver(IDatabaseService database) : IPrefixResolver
{
    public async ValueTask<int> ResolvePrefixAsync(CommandsExtension extension, DiscordMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return -1;
        }
        // Mention check
        else if (this.AllowMention && message.Content.StartsWith(extension.Client.CurrentUser.Mention, StringComparison.OrdinalIgnoreCase))
        {
            return extension.Client.CurrentUser.Mention.Length;
        }

        // Database check
        string prefix = await database.GetPrefixAsync(message.Channel.GuildId);
        if (message.Content.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return prefix.Length;
        }

        return -1;
    }
}
```

Now, unlike the normal prefix resolver delegate, this isn't set on the TextCommandConfiguration. Instead, you'll register this class with your service provider and the text command processor will use it by default, even if the prefix resolver delegate is already set somewhere else.

> [!IMPORTANT]
> The prefix resolver is resolved from a scoped service provider. For most scenarios, the only stipulation is that state should be held in an external, more persistent (e.g. singleton) service. Users of Entity Framework Core (EFCore) should ensure that the DbContext is scoped correctly.

```cs
DiscordClientBuilder builder = DiscordClientBuilder
    .CreateDefault(discordToken, TextCommandProcessor.RequiredIntents | SlashCommandProcessor.RequiredIntents)
    .ConfigureServices(services => services.AddScoped<IPrefixResolver, CustomPrefixResolver>());
```

And just like that, you're off to the races.
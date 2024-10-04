---
uid: articles.commands.command_processors.slash_commands.translator_providers
title: Translator Providers
---

# Translator Providers

In the event that you would like to provide translations for your commands, you can use the `ITranslatorProvider` interface. This interface allows you to provide translations for your commands and parameters. Here is the interface:

```cs
public interface IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName);
}
```

`fullSymbolName` is a special id you can use to index your translations. Below is the formatting used:

- `command.name`: The full name of the command (`group.subgroup.command.name`).
- `command.description`: The description of the command.
- `command.parameter.name`: The name of the parameter.
- `command.parameter.description`: The description of the parameter.

Here is an example of a simple translator provider:

```cs
public class PingTranslator : IInteractionLocalizer
{
    public ValueTask<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName) => fullSymbolName switch
    {
        "ping.name" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "ping" },
            { DiscordLocale.ja, "ピン" },
            { DiscordLocale.tr, "ping" }
        }),
        "ping.description" => ValueTask.FromResult<IReadOnlyDictionary<DiscordLocale, string>>(new Dictionary<DiscordLocale, string>
        {
            { DiscordLocale.en_US, "Pings the bot to check its latency." },
            { DiscordLocale.ja, "ボットにピンを送信して、その遅延を確認します。" },
            { DiscordLocale.tr, "Botun gecikmesini kontrol eder." }
        }),
        _ => throw new KeyNotFoundException()
    };
}
```

You can then use this translator provider in your command registration:

```cs
public static class PingCommand
{
    [Command("ping"), InteractionLocalizer<PingTranslator>]
    public static async ValueTask ExecuteAsync(CommandContext context) => await context.RespondAsync("Pong!");
}
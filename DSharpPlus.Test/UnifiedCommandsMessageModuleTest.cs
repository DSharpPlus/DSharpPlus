using System;
using System.Threading.Tasks;
using DSharpPlus.UnifiedCommands.Message;
using DSharpPlus.UnifiedCommands.Message.Conditions;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Test;

[MessageModule("test")]
public class UnifiedCommandsMessageModuleTest : MessageModule
{
    // This is commented out until I have implemented default converters for all other types
    /* private readonly string _str;

    public UnifiedCommandsMessageModuleTest(string str) => _str = str;

    [Message("sync")]
    public IMessageResult TestSync() => Reply("Sync works.");

    [Message("async")]
    public async Task<IMessageResult> TestAsync()
    {
        await PostAsync(Reply("Async works"));
        return Empty();
    }

    [Message("arg opt")]
    public IMessageResult TestArgOpt(string argument,
        [MessageOption("user", "u")] Entities.DiscordUser? user, [MessageOption("string", "s")] string str = "hello")
        => Reply(
            user is not null
                ? $"Argument was `{argument}`, user {user.Username}, and string was `{str}`."
                : $"Argument was `{argument}`, user wasn't provided, and string was `{str}`.");


    [Message("permissions")]
    [MessagePermission(Permissions.Administrator)]
    public IMessageResult TestPermissions() => Reply("You are a admin.");

    [Message("di")]
    public IMessageResult TestDi() => Reply($"DI gave me value `{_str}`.");

    [Message("no value")]
    public async ValueTask TestNoValueAsync()
    {
        await PostAsync(Reply("This returns nothing."));
        return;
    }

    [Message("remaining arguments")]
    public IMessageResult TestRemainingArguments([RemainingArguments] string arguments,
        [MessageOption("str", "s")] string? str)
        => Reply(str is null
            ? $"Remaining arguments is `{arguments}`. Str is null"
            : $"Remaining arguments is `{arguments}`. Str is `{str}`.");

    [Message("cooldown"), Cooldown(10)]
    public IMessageResult TestCooldowns()
        => Reply("No cooldown.");

    [Message("failing")]
    public IMessageResult TestFailing()
        => throw new Exception("Fuck you"); */

    [Message("simple-str")]
    public IMessageResult TestSimpleStr([MessageOption("str")] string str)
    {
        IMessageResult result = Reply(str);
        Client.Logger.LogInformation("Got str \"{String}\"", result.Builder.Content);
        return result;
    }
}

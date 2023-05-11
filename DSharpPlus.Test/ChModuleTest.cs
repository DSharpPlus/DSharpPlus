using System;
using System.Threading.Tasks;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Message.Conditions;

namespace DSharpPlus.Test;

[MessageModule("test")]
public class ChModuleTest : MessageModule
{
    private readonly string _str;

    public ChModuleTest(string str) => _str = str;

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

    [Message("emojis")]
    public async Task<IMessageResult> TestEmojisAsync()
    {
        await PostAsync(Reply("React with a emoji, author can only reply"));
        EventArgs.MessageReactionAddEventArgs? args = await WaitForReactionAsync(TimeSpan.FromSeconds(10),
            (e) => e.User.Id == Message.Author.Id);

        if (args is not null)
        {
            return FollowUp($"Reacted with emoji {args.Emoji.Name}");
        }
        else
        {
            return FollowUp($"Duration ran out.");
        }
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
        => throw new Exception("Fuck you");
}

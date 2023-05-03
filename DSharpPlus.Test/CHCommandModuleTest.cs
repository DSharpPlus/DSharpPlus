using System;
using System.Threading.Tasks;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Message.Permission;

namespace DSharpPlus.Test;

[MessageModule("test")]
public class CHCommandModuleTest : MessageCommandModule
{
    private readonly string _str;

    public CHCommandModuleTest(string str) => _str = str;

    [MessageCommand("sync")]
    public IMessageCommandModuleResult TestSync() => Reply("Sync works.");

    [MessageCommand("async")]
    public async Task<IMessageCommandModuleResult> TestAsync()
    {
        await PostAsync(Reply("Async works"));
        return Empty();
    }

    [MessageCommand("arg opt")]
    public IMessageCommandModuleResult TestArgOpt(string argument,
        [MessageOption("user", "u")] Entities.DiscordUser? user, [MessageOption("string", "s")] string str = "hello")
    {
        return Reply(
            user is not null
                ? $"Argument was `{argument}`, user {user.Username}, and string was `{str}`."
                : $"Argument was `{argument}`, user wasn't provided, and string was `{str}`.");
    }

    [MessageCommand("permissions")]
    [MessagePermission(Permissions.Administrator)]
    public IMessageCommandModuleResult TestPermissions() => Reply("You are a admin.");

    [MessageCommand("di")]
    public IMessageCommandModuleResult TestDi() => Reply($"DI gave me value `{_str}`.");

    [MessageCommand("no value")]
    public async Task TestNoValueAsync()
    {
        await PostAsync(Reply("This returns nothing."));
        return;
    }

    [MessageCommand("emojis")]
    public async Task<IMessageCommandModuleResult> TestEmojisAsync()
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
}

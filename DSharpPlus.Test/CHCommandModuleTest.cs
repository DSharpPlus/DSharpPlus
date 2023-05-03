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

    [MessageCommand("emote")]
    public async Task<IMessageCommandModuleResult> TestEmoteAsync()
    {
        await PostAsync(Reply("React to this message for a reply."));

        EventArgs.MessageReactionAddEventArgs? reaction = await WaitForReactionAsync(TimeSpan.FromSeconds(5));
        if (reaction is null)
        {
            return FollowUp("No reactions happened.");
        }
        else
        {
            return FollowUp($"You reacted with emoji {reaction.Emoji.Name}");
        }
    }
}

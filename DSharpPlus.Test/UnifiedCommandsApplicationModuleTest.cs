using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Application;
using DSharpPlus.UnifiedCommands.Application.Internals;

namespace DSharpPlus.Test;

[ApplicationModule("app", "Random application commands.")]
public class UnifiedCommandsApplicationModuleTest : ApplicationModule
{
    [ApplicationName("reply-test", "This is a reply test")]
    public IApplicationResult ReplyTest()
    {
        DiscordEmbedBuilder builder = new();
        builder.WithTitle("Hello, world!").WithDescription("This is a test. Thank you for joining in!");
        return Reply(builder);
    }

    [ApplicationName("reply-opt-async", "This has options and replies async with a followup.")]
    public async Task<IApplicationResult> ReplyOptAsync([ApplicationOption("user", "A user")] DiscordUser user)
    {
        await PostAsync(Reply("Test test."));
        return FollowUp($"You selected user {Formatter.Mention(user)}");
    }
}

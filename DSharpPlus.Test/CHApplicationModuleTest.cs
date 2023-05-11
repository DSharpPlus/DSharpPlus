using System.Threading.Tasks;
using DSharpPlus.CH.Application;
using DSharpPlus.CH.Application.Internals;
using DSharpPlus.Entities;

namespace DSharpPlus.Test;

[ApplicationModule]
public class CHApplicationModuleTest : ApplicationModule
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

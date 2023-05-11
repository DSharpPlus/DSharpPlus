using DSharpPlus.CH.Application.Internals;
using DSharpPlus.Entities;

namespace DSharpPlus.CH.Application;

public abstract class ApplicationModule
{
    internal ApplicationHandler _handler = null!;

    public DiscordClient Client { get; internal set; } = null!;
    public DiscordInteraction Interaction { get; internal set; } = null!;

    protected Task PostAsync(IApplicationResult result)
        => _handler.TurnResultIntoActionAsync(result);

    protected IApplicationResult Reply(ApplicationResult result)
    {
        result.Type = ApplicationResultType.Reply;
        return result;
    }

    protected IApplicationResult FollowUp(ApplicationResult result)
    {
        result.Type = ApplicationResultType.FollowUp;
        return result;
    }

    protected Task<DiscordMessage> GetOriginalResponseAsync()
        => Interaction.GetOriginalResponseAsync();
}

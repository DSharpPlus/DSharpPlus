using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Application.Conditions;

public interface IApplicationCondition
{
    public Task<bool> InvokeAsync(DiscordInteraction interaction, DiscordClient client);
}

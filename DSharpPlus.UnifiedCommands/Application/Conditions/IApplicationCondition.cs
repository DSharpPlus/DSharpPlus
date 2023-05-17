using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Application.Conditions;

public interface IApplicationCondition
{
    public ValueTask<bool> InvokeAsync(DiscordInteraction interaction, DiscordClient client);
}

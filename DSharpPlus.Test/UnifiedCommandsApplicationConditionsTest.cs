using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Application.Conditions;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Test;

public class UnifiedCommandsApplicationConditionsTest : IApplicationCondition
{
    public ValueTask<bool> InvokeAsync(DiscordInteraction _, DiscordClient client)
    {
        client.Logger.LogInformation("This got passed through this pass");
        return ValueTask.FromResult(true);
    }
}

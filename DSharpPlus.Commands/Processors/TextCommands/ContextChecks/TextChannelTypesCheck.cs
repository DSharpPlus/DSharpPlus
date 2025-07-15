using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

/// <summary>
/// Implements a check for channel types on text commands.
/// </summary>
internal sealed class TextChannelTypesCheck : IParameterCheck<ChannelTypesAttribute>
{
    /// <inheritdoc/>
    public ValueTask<string?> ExecuteCheckAsync(ChannelTypesAttribute attribute, ParameterCheckInfo info, CommandContext context)
    {
        if (info.Value is not DiscordChannel channel)
        {
            return ValueTask.FromResult<string?>(null);
        }
        else if (attribute.ChannelTypes.Contains(channel.Type))
        {
            return ValueTask.FromResult<string?>(null);
        }

        return ValueTask.FromResult<string?>("The specified channel was not of one of the required types.");
    }
}

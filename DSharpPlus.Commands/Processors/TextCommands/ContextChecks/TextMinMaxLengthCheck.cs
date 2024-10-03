using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks.ParameterChecks;

namespace DSharpPlus.Commands.Processors.TextCommands.ContextChecks;

/// <summary>
/// Implements min/max length checks for strings on text commands.
/// </summary>
internal sealed class TextMinMaxLengthCheck : IParameterCheck<MinMaxLengthAttribute>
{
    /// <inheritdoc/>
    public ValueTask<string?> ExecuteCheckAsync(MinMaxLengthAttribute attribute, ParameterCheckInfo info, CommandContext context)
    {
        if (info.Value is not string value)
        {
            return ValueTask.FromResult<string?>(null);
        }
        else if (value.Length < attribute.MinLength)
        {
            return ValueTask.FromResult<string?>($"The supplied string was too short, expected a minimum length of {attribute.MinLength}.");
        }
        else if (value.Length > attribute.MaxLength)
        {
            return ValueTask.FromResult<string?>($"The supplied string was too long, expected a maximum length of {attribute.MaxLength}.");
        }

        return ValueTask.FromResult<string?>(null);
    }
}

using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class BooleanConverter : ISlashArgumentConverter<bool>, ITextArgumentConverter<bool>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Boolean;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Boolean (true/false)";

    /// <inheritdoc/>
    public Task<Optional<bool>> ConvertAsync(ConverterContext context) => Task.FromResult(context.Argument?.ToString()?.ToLowerInvariant() switch
    {
        "true" or "yes" or "y" or "1" or "on" or "enable" or "enabled" or "t" => Optional.FromValue(true),
        "false" or "no" or "n" or "0" or "off" or "disable" or "disabled" or "f" => Optional.FromValue(false),
        _ => Optional.FromNoValue<bool>(),
    });
}

using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class BooleanConverter : ISlashArgumentConverter<bool>, ITextArgumentConverter<bool>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Boolean;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<bool>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => Task.FromResult(context.Argument.ToLowerInvariant() switch
    {
        "true" or "yes" or "y" or "1" or "on" or "enable" or "enabled" or "t" => Optional.FromValue(true),
        "false" or "no" or "n" or "0" or "off" or "disable" or "disabled" or "f" => Optional.FromValue(false),
        _ => Optional.FromNoValue<bool>()
    });

    public Task<Optional<bool>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => bool.TryParse(context.Argument.RawValue, out bool result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<bool>());
}

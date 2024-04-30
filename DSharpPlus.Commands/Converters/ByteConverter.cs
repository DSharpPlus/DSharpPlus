namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class ByteConverter : ISlashArgumentConverter<byte>, ITextArgumentConverter<byte>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<byte>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => byte.TryParse(context.Argument, CultureInfo.InvariantCulture, out byte result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<byte>());

    public Task<Optional<byte>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => byte.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out byte result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<byte>());
}

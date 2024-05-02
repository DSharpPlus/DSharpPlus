namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class UInt16Converter : ISlashArgumentConverter<ushort>, ITextArgumentConverter<ushort>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Unsigned Short Integer (0 through 65535)";
    public bool RequiresText => true;

    public Task<Optional<ushort>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => ushort.TryParse(context.Argument, CultureInfo.InvariantCulture, out ushort result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<ushort>());

    public Task<Optional<ushort>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => ushort.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ushort result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<ushort>());
}

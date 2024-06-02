using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class UInt16Converter : ISlashArgumentConverter<ushort>, ITextArgumentConverter<ushort>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Positive Small Integer";
    public bool RequiresText => true;

    public Task<Optional<ushort>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => ushort.TryParse(context.Argument, CultureInfo.InvariantCulture, out ushort result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<ushort>());

    public Task<Optional<ushort>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => ushort.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out ushort result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<ushort>());
}

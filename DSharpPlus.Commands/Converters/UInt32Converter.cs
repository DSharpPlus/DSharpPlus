using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class UInt32Converter : ISlashArgumentConverter<uint>, ITextArgumentConverter<uint>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Positive Integer";
    public bool RequiresText => true;

    public Task<Optional<uint>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => uint.TryParse(context.Argument, CultureInfo.InvariantCulture, out uint result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<uint>());

    public Task<Optional<uint>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => uint.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out uint result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<uint>());
}

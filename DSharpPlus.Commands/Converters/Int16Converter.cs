using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class Int16Converter : ISlashArgumentConverter<short>, ITextArgumentConverter<short>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Small Integer";
    public bool RequiresText => true;

    public Task<Optional<short>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => short.TryParse(context.Argument, CultureInfo.InvariantCulture, out short result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<short>());

    public Task<Optional<short>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => short.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out short result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<short>());
}

namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class Int16Converter : ISlashArgumentConverter<short>, ITextArgumentConverter<short>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<short>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => short.TryParse(context.Argument, CultureInfo.InvariantCulture, out short result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<short>());

    public Task<Optional<short>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => short.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out short result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<short>());
}

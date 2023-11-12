using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters;

public class Int16Converter : ISlashArgumentConverter<short>, ITextArgumentConverter<short>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<short>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        short.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out short result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<short>());

    public Task<Optional<short>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
        short.TryParse(context.As<SlashConverterContext>().Argument.Value.ToString(), out short result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<short>());
}

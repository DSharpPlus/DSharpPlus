using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters;

public class Int64Converter : ISlashArgumentConverter<long>, ITextArgumentConverter<long>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; }

    public Task<Optional<long>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        long.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out long result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<long>());

    public Task<Optional<long>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
        long.TryParse(context.As<SlashConverterContext>().Argument.Value.ToString(), out long result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<long>());
}

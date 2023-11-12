namespace DSharpPlus.CommandAll.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class UInt64Converter : ISlashArgumentConverter<ulong>, ITextArgumentConverter<ulong>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<ulong>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        ulong.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out ulong result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ulong>());

    public Task<Optional<ulong>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
        ulong.TryParse(context.As<SlashConverterContext>().Argument.Value.ToString(), out ulong result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ulong>());
}

namespace DSharpPlus.CommandAll.Converters;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class UInt16Converter : ISlashArgumentConverter<ushort>, ITextArgumentConverter<ushort>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<ushort>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        ushort.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out ushort result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ushort>());

    public Task<Optional<ushort>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
        ushort.TryParse(context.As<SlashConverterContext>().Argument.Value.ToString(), out ushort result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ushort>());
}

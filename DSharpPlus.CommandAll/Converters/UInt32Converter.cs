namespace DSharpPlus.CommandAll.Converters;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class UInt32Converter : ISlashArgumentConverter<uint>, ITextArgumentConverter<uint>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<uint>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        uint.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out uint result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<uint>());

    public Task<Optional<uint>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((uint)context.As<SlashConverterContext>().Argument.Value));
}

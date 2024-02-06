namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
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

    public Task<Optional<ushort>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => context.As<SlashConverterContext>().Argument.Value is int number && number >= ushort.MinValue && number <= ushort.MaxValue
        ? Task.FromResult(Optional.FromValue((ushort)number))
        : Task.FromResult(Optional.FromNoValue<ushort>());
}

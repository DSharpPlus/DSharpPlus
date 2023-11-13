namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class ByteConverter : ISlashArgumentConverter<byte>, ITextArgumentConverter<byte>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<byte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => byte.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out byte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<byte>());

    public Task<Optional<byte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => context.As<SlashConverterContext>().Argument.Value is int number && number >= byte.MinValue && number <= byte.MaxValue
            ? Task.FromResult(Optional.FromValue((byte)number))
            : Task.FromResult(Optional.FromNoValue<byte>());
}

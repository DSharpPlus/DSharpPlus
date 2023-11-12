namespace DSharpPlus.CommandAll.Converters;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class ByteConverter : ISlashArgumentConverter<byte>, ITextArgumentConverter<byte>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<byte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context.As<TextConverterContext>().Argument);
    public Task<Optional<byte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context.As<SlashConverterContext>().Argument.ToString());
    public static Task<Optional<byte>> ConvertAsync(string? value) =>
        byte.TryParse(value, CultureInfo.InvariantCulture, out byte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<byte>());
}

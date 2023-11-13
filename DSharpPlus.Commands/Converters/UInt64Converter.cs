namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class UInt64Converter : ISlashArgumentConverter<ulong>, ITextArgumentConverter<ulong>
{
    // Discord:              9,007,199,254,740,992
    // UInt64.MaxValue: 18,446,744,073,709,551,615
    // The input is defined as a string to allow for the use of the "ulong" type.
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.String;
    public bool RequiresText { get; init; }

    public Task<Optional<ulong>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context.As<TextConverterContext>().Argument);
    public Task<Optional<ulong>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync((string)context.As<SlashConverterContext>().Argument.Value);
    public static Task<Optional<ulong>> ConvertAsync(string? value) =>
        ulong.TryParse(value, CultureInfo.InvariantCulture, out ulong result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ulong>());
}

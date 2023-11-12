namespace DSharpPlus.CommandAll.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class Int32Converter : ISlashArgumentConverter<int>, ITextArgumentConverter<int>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<int>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        int.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out int result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<int>());

    public Task<Optional<int>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((int)context.As<SlashConverterContext>().Argument.Value));
}

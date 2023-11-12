using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters;

public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
        sbyte.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out sbyte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<sbyte>());

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
        sbyte.TryParse(context.As<SlashConverterContext>().Argument.Value.ToString(), out sbyte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<sbyte>());
}

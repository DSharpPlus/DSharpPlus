namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<sbyte>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => sbyte.TryParse(context.Argument, CultureInfo.InvariantCulture, out sbyte result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<sbyte>());

    public Task<Optional<sbyte>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => sbyte.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out sbyte result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<sbyte>());
}

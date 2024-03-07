namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class FloatConverter : ISlashArgumentConverter<float>, ITextArgumentConverter<float>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Number;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<float>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => float.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out float result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<float>());

    public Task<Optional<float>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => float.TryParse(context.As<InteractionConverterContext>().Argument.RawValue, CultureInfo.InvariantCulture, out float result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<float>());
}

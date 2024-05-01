namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class DoubleConverter : ISlashArgumentConverter<double>, ITextArgumentConverter<double>
{
    public DiscordApplicationCommandOptionType ParameterType { get; init; } = DiscordApplicationCommandOptionType.Number;
    public string ReadableName { get; init; } = "Decimal Number";
    public bool RequiresText { get; init; } = true;

    public Task<Optional<double>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => double.TryParse(context.Argument, CultureInfo.InvariantCulture, out double result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<double>());

    public Task<Optional<double>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => double.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out double result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<double>());
}

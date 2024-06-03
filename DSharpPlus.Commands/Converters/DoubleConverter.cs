using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class DoubleConverter : ISlashArgumentConverter<double>, ITextArgumentConverter<double>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Number;
    public string ReadableName => "Decimal Number";
    public bool RequiresText => true;

    public Task<Optional<double>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => double.TryParse(context.Argument, CultureInfo.InvariantCulture, out double result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<double>());

    public Task<Optional<double>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => double.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out double result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<double>());
}

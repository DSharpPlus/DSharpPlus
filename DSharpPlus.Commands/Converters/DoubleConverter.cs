using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class DoubleConverter : ISlashArgumentConverter<double>, ITextArgumentConverter<double>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Number;
    public string ReadableName => "Decimal Number";
    public bool RequiresText => true;

    public Task<Optional<double>> ConvertAsync(ConverterContext context) => double.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out double result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<double>());
}

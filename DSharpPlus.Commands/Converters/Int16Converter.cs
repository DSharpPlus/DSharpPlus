using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class Int16Converter : ISlashArgumentConverter<short>, ITextArgumentConverter<short>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Small Integer";

    public Task<Optional<short>> ConvertAsync(ConverterContext context) =>
        short.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out short result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<short>());
}

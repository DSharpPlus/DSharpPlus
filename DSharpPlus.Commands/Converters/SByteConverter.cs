using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Tiny Integer";

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context) =>
        sbyte.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out sbyte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<sbyte>());
}

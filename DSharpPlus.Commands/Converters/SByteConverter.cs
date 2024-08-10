using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Tiny Integer";
    public bool RequiresText => true;

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context) => sbyte.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out sbyte result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<sbyte>());
}

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class UInt64Converter : ISlashArgumentConverter<ulong>, ITextArgumentConverter<ulong>
{
    // Discord:              9,007,199,254,740,992
    // UInt64.MaxValue: 18,446,744,073,709,551,615
    // The input is defined as a string to allow for the use of the "ulong" type.
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterRequiresText RequiresText => ConverterRequiresText.Always;
    public string ReadableName => "Positive Large Integer";

    public Task<Optional<ulong>> ConvertAsync(ConverterContext context) =>
        ulong.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out ulong result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<ulong>());
}

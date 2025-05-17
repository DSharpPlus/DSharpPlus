using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class Int64Converter : ISlashArgumentConverter<long>, ITextArgumentConverter<long>
{
    // Discord:            9,007,199,254,740,992
    // Int64.MaxValue: 9,223,372,036,854,775,807
    // The input is defined as a string to allow for the use of the "long" type.
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Large Integer";

    public Task<Optional<long>> ConvertAsync(ConverterContext context) =>
        long.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out long result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<long>());
}

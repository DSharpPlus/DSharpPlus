using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class ByteConverter : ISlashArgumentConverter<byte>, ITextArgumentConverter<byte>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Positive Tiny Integer";

    public Task<Optional<byte>> ConvertAsync(ConverterContext context) =>
        byte.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out byte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<byte>());
}

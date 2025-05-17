using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class Int32Converter : ISlashArgumentConverter<int>, ITextArgumentConverter<int>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Integer";

    public Task<Optional<int>> ConvertAsync(ConverterContext context) =>
        int.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, out int result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<int>());
}

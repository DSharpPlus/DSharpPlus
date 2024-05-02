namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class Int64Converter : ISlashArgumentConverter<long>, ITextArgumentConverter<long>
{
    // Discord:            9,007,199,254,740,992
    // Int64.MaxValue: 9,223,372,036,854,775,807
    // The input is defined as a string to allow for the use of the "long" type.
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public string ReadableName => "Long Integer (0 through 9,223,372,036,854,775,807)";
    public bool RequiresText => true;

    public Task<Optional<long>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => long.TryParse(context.Argument, CultureInfo.InvariantCulture, out long result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<long>());

    public Task<Optional<long>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => long.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out long result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<long>());
}

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class FloatConverter : ISlashArgumentConverter<float>, ITextArgumentConverter<float>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Number;
    public string ReadableName => "Decimal Number";
    public bool RequiresText => true;

    public Task<Optional<float>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => float.TryParse(context.Argument, CultureInfo.InvariantCulture, out float result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<float>());

    public Task<Optional<float>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => float.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out float result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<float>());
}

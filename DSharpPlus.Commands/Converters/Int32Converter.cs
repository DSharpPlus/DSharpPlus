using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class Int32Converter : ISlashArgumentConverter<int>, ITextArgumentConverter<int>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Integer";
    public bool RequiresText => true;

    public Task<Optional<int>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => int.TryParse(context.Argument, CultureInfo.InvariantCulture, out int result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<int>());

    public Task<Optional<int>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => int.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out int result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<int>());
}

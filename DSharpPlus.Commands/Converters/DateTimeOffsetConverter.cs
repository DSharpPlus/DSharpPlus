using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class DateTimeOffsetConverter : ISlashArgumentConverter<DateTimeOffset>, ITextArgumentConverter<DateTimeOffset>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public string ReadableName => "Date and Time";
    public bool RequiresText => true;

    public Task<Optional<DateTimeOffset>> ConvertAsync(TextConverterContext context, MessageCreatedEventArgs eventArgs) => DateTimeOffset.TryParse(context.Argument, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());

    public Task<Optional<DateTimeOffset>> ConvertAsync(InteractionConverterContext context, InteractionCreatedEventArgs eventArgs) => DateTimeOffset.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

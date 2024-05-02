namespace DSharpPlus.Commands.Converters;

using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class DateTimeOffsetConverter : ISlashArgumentConverter<DateTimeOffset>, ITextArgumentConverter<DateTimeOffset>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public string ReadableName => "Date and Time";
    public bool RequiresText => true;

    public Task<Optional<DateTimeOffset>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => DateTimeOffset.TryParse(context.Argument, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());

    public Task<Optional<DateTimeOffset>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs) => DateTimeOffset.TryParse(context.Argument.RawValue, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

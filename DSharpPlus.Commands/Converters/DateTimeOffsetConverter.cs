using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class DateTimeOffsetConverter : ISlashArgumentConverter<DateTimeOffset>, ITextArgumentConverter<DateTimeOffset>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
    public ConverterInputType RequiresText => ConverterInputType.Always;
    public string ReadableName => "Date and Time";

    public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context) =>
        DateTimeOffset.TryParse(context.Argument?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

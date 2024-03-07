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
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => DateTimeOffset.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());

    public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => DateTimeOffset.TryParse(context.As<InteractionConverterContext>().Argument.RawValue, CultureInfo.InvariantCulture, out DateTimeOffset result)
        ? Task.FromResult(Optional.FromValue(result))
        : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

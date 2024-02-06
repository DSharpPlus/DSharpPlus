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

    public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context.As<TextConverterContext>().Argument);
    public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context.As<SlashConverterContext>().Argument.ToString());
    public static Task<Optional<DateTimeOffset>> ConvertAsync(string? value) =>
        DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out DateTimeOffset result)
            ? Task.FromResult(Optional.FromValue(result.ToUniversalTime()))
            : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
}

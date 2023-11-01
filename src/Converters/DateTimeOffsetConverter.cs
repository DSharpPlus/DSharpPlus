using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DateTimeOffsetConverter : ISlashArgumentConverter<DateTimeOffset>, ITextArgumentConverter<DateTimeOffset>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.String;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context.As<TextConverterContext>().CurrentTextArgument);
        public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context.As<SlashConverterContext>().CurrentOption.Value.ToString());
        public static Task<Optional<DateTimeOffset>> ConvertAsync(string? value) =>
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out DateTimeOffset result)
                ? Task.FromResult(Optional.FromValue(result.ToUniversalTime()))
                : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
    }
}

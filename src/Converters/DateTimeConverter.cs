using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class DateTimeConverter : ISlashArgumentConverter<DateTime>, ITextArgumentConverter<DateTime>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.String;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<DateTime>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => ConvertAsync(context.As<TextConverterContext>().CurrentTextArgument);
        public Task<Optional<DateTime>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => ConvertAsync(context.As<SlashConverterContext>().CurrentOption.Value.ToString());
        public static Task<Optional<DateTime>> ConvertAsync(string? value) =>
            DateTime.TryParse(value, CultureInfo.InvariantCulture, out DateTime result)
                ? Task.FromResult(Optional.FromValue(result.ToUniversalTime()))
                : Task.FromResult(Optional.FromNoValue<DateTime>());
    }
}

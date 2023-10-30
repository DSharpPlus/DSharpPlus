using System;
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

        public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new NotImplementedException();
        public Task<Optional<DateTimeOffset>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            DateTimeOffset.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out DateTimeOffset result)
                ? Task.FromResult(Optional.FromValue(result.ToUniversalTime()))
                : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
    }
}

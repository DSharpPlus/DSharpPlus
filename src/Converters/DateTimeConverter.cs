using System;
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

        public Task<Optional<DateTime>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new NotImplementedException();
        public Task<Optional<DateTime>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            DateTime.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out DateTime result)
                ? Task.FromResult(Optional.FromValue(result.ToUniversalTime()))
                : Task.FromResult(Optional.FromNoValue<DateTime>());
    }
}

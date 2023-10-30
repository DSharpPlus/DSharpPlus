using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class Int64Converter : ISlashArgumentConverter<long>, ITextArgumentConverter<long>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;

        public Task<Optional<long>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<long>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            long.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out long result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<long>());
    }
}

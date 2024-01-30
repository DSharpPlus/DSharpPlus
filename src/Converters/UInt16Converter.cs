using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class UInt16Converter : ISlashArgumentConverter<ushort>, ITextArgumentConverter<ushort>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;

        public Task<Optional<ushort>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<ushort>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            ushort.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out ushort result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<ushort>());
    }
}

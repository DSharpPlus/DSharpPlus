using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class ByteConverter : ISlashArgumentConverter<byte>, ITextArgumentConverter<byte>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;

        public Task<Optional<byte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<byte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            byte.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out byte result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<byte>());
    }
}

using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class Int32Converter : ISlashArgumentConverter<int>, ITextArgumentConverter<int>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;

        public Task<Optional<int>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => throw new System.NotImplementedException();
        public Task<Optional<int>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((int)context.As<SlashConverterContext>().CurrentOption.Value));
    }
}

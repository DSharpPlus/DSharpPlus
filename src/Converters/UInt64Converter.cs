using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class UInt64Converter : ISlashArgumentConverter<ulong>, ITextArgumentConverter<ulong>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<ulong>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
            ulong.TryParse(context.As<TextConverterContext>().CurrentTextArgument, CultureInfo.InvariantCulture, out ulong result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<ulong>());

        public Task<Optional<ulong>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            ulong.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out ulong result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<ulong>());
    }
}

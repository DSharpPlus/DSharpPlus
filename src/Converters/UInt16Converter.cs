using System.Globalization;
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
        public bool RequiresText { get; init; } = true;

        public Task<Optional<ushort>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
            ushort.TryParse(context.As<TextConverterContext>().CurrentTextArgument, CultureInfo.InvariantCulture, out ushort result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<ushort>());

        public Task<Optional<ushort>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            ushort.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out ushort result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<ushort>());
    }
}

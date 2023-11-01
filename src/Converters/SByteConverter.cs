using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Integer;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
            sbyte.TryParse(context.As<TextConverterContext>().CurrentTextArgument, CultureInfo.InvariantCulture, out sbyte result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<sbyte>());

        public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) =>
            sbyte.TryParse(context.As<SlashConverterContext>().CurrentOption.Value.ToString(), out sbyte result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<sbyte>());
    }
}

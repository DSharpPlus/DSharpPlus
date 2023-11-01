using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class FloatConverter : ISlashArgumentConverter<float>, ITextArgumentConverter<float>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.Number;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<float>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) =>
            float.TryParse(context.As<TextConverterContext>().CurrentTextArgument, CultureInfo.InvariantCulture, out float result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<float>());

        public Task<Optional<float>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((float)context.As<SlashConverterContext>().CurrentOption.Value));
    }
}

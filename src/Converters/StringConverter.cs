using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class StringConverter : ISlashArgumentConverter<string>, ITextArgumentConverter<string>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.String;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<string>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
        {
            TextConverterContext textContext = context.As<TextConverterContext>();
            if (!context.Argument.Attributes.Any(attribute => attribute is RemainingTextAttribute))
            {
                return Task.FromResult(Optional.FromValue(textContext.CurrentTextArgument));
            }

            string currentTextArgument = textContext.CurrentTextArgument;
            int nextIndex = textContext.NextTextIndex;
            while (textContext.NextTextArgument()) { }
            return Task.FromResult(Optional.FromValue(currentTextArgument + textContext.RawArguments[nextIndex..]));
        }

        public Task<Optional<string>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => eventArgs.Interaction.Type != InteractionType.ApplicationCommand
            ? Task.FromResult(Optional.FromNoValue<string>())
            : Task.FromResult(Optional.FromValue(eventArgs.Interaction.Data.Options.ElementAt(context.ArgumentIndex).Value.ToString()!));
    }
}

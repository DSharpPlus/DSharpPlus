using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.CommandAll.Converters
{
    public class StringConverter : ISlashArgumentConverter<string>, ITextArgumentConverter<string>
    {
        public ApplicationCommandOptionType ArgumentType { get; init; } = ApplicationCommandOptionType.String;
        public bool RequiresText { get; init; } = true;

        public Task<Optional<string>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue(context.As<TextConverterContext>().CurrentTextArgument));
        public Task<Optional<string>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => eventArgs.Interaction.Type != InteractionType.ApplicationCommand
            ? Task.FromResult(Optional.FromNoValue<string>())
            : Task.FromResult(Optional.FromValue(eventArgs.Interaction.Data.Options.ElementAt(context.ArgumentIndex).Value.ToString()!));
    }
}

namespace DSharpPlus.CommandAll.Converters;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class StringConverter : ISlashArgumentConverter<string>, ITextArgumentConverter<string>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.String;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<string>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs)
    {
        TextConverterContext textContext = context.As<TextConverterContext>();
        if (!context.Parameter.Attributes.Any(attribute => attribute is RemainingTextAttribute))
        {
            return Task.FromResult(Optional.FromValue(textContext.Argument));
        }

        TextConverterContext textConverterContext = context.As<TextConverterContext>();
        return Task.FromResult(Optional.FromValue(textConverterContext.RawArguments[textConverterContext.CurrentArgumentIndex..]));
    }

    public Task<Optional<string>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => eventArgs.Interaction.Type != InteractionType.ApplicationCommand
        ? Task.FromResult(Optional.FromNoValue<string>())
        : Task.FromResult(Optional.FromValue(eventArgs.Interaction.Data.Options.ElementAt(context.ParameterIndex).Value.ToString()!));
}

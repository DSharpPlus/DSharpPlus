namespace DSharpPlus.Commands.Converters;

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
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

    public Task<Optional<string>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => Task.FromResult(Optional.FromValue((string)context.As<SlashConverterContext>().Argument.Value));
}

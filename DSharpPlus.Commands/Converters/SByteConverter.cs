namespace DSharpPlus.Commands.Converters;

using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class SByteConverter : ISlashArgumentConverter<sbyte>, ITextArgumentConverter<sbyte>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, MessageCreateEventArgs eventArgs) => sbyte.TryParse(context.As<TextConverterContext>().Argument, CultureInfo.InvariantCulture, out sbyte result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<sbyte>());

    public Task<Optional<sbyte>> ConvertAsync(ConverterContext context, InteractionCreateEventArgs eventArgs) => context.As<SlashConverterContext>().Argument.Value is int number && number >= sbyte.MinValue && number <= sbyte.MaxValue
            ? Task.FromResult(Optional.FromValue((sbyte)number))
            : Task.FromResult(Optional.FromNoValue<sbyte>());
}

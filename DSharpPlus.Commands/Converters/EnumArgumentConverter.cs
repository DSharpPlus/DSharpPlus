namespace DSharpPlus.Commands.Converters;

using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
{
    public ApplicationCommandOptionType ParameterType { get; init; } = ApplicationCommandOptionType.Integer;
    public bool RequiresText { get; init; } = true;

    public Task<Optional<Enum>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs) => Task.FromResult(Enum.TryParse(context.Parameter.Type, context.Argument, true, out object? result)
        ? Optional.FromValue((Enum)result)
        : Optional.FromNoValue<Enum>());

    public Task<Optional<Enum>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        object value = Convert.ChangeType(context.Argument.Value, Enum.GetUnderlyingType(context.Parameter.Type), CultureInfo.InvariantCulture);
        return Enum.IsDefined(context.Parameter.Type, value)
            ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(context.Parameter.Type, value)))
            : Task.FromResult(Optional.FromNoValue<Enum>());
    }
}

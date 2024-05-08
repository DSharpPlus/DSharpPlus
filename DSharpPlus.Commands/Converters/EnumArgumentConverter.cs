using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Commands.Converters;

public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Multiple Choice";
    public bool RequiresText => true;

    public Task<Optional<Enum>> ConvertAsync(TextConverterContext context, MessageCreateEventArgs eventArgs)
    {
        return Task.FromResult(Enum.TryParse(context.Parameter.Type, context.Argument, true, out object? result)
            ? Optional.FromValue((Enum)result)
            : Optional.FromNoValue<Enum>());
    }

    public Task<Optional<Enum>> ConvertAsync(InteractionConverterContext context, InteractionCreateEventArgs eventArgs)
    {
        Type effectiveType = Nullable.GetUnderlyingType(context.Parameter.Type) ?? context.Parameter.Type;

        object value = Convert.ChangeType
        (
            context.Argument.Value,
            Enum.GetUnderlyingType(effectiveType),
            CultureInfo.InvariantCulture
        );

        return Enum.IsDefined(effectiveType, value)
            ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(effectiveType, value)))
            : Task.FromResult(Optional.FromNoValue<Enum>());
    }
}

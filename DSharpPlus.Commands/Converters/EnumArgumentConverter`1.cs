using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class EnumConverter<T> : ISlashArgumentConverter<T>, ITextArgumentConverter<T> where T : struct, Enum
{
    private static readonly Type baseEnumType = Enum.GetUnderlyingType(typeof(T));

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public ConverterRequiresText RequiresText => ConverterRequiresText.Always;
    public string ReadableName => "Multiple Choice";

    public Task<Optional<T>> ConvertAsync(ConverterContext context)
    {
        // Null check for nullability warnings, however I think this is redundant as the base processor should handle this
        if (context.Argument is null)
        {
            return Task.FromResult(Optional.FromNoValue<T>());
        }
        else if (context.Argument is string stringArgument)
        {
            return Enum.TryParse(stringArgument, true, out T result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<T>());
        }

        // Convert the argument to the base type of the enum. If this was invoked via slash commands,
        // Discord will send us the argument as a number, which STJ will convert to an unknown numeric type.
        // We need to ensure that the argument is the same type as it's enum base type.
        // Convert the argument to the base type of the enum. If this was invoked via slash commands,
        // Discord will send us the argument as a number, which STJ will convert to an unknown numeric type.
        // We need to ensure that the argument is the same type as it's enum base type.
        T value = (T)Convert.ChangeType(context.Argument, baseEnumType, CultureInfo.InvariantCulture);
        return Enum.IsDefined(value)
            ? Task.FromResult(Optional.FromValue(value))
            : Task.FromResult(Optional.FromNoValue<T>());
    }
}

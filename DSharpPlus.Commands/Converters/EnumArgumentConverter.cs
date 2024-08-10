using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
{
    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Multiple Choice";
    public bool RequiresText => true;

    public Task<Optional<Enum>> ConvertAsync(ConverterContext context)
    {
        if (context.Argument is string stringArgument && Enum.TryParse(context.Parameter.Type, stringArgument, true, out object? result))
        {
            return Task.FromResult(Optional.FromValue((Enum)result));
        }

        // The parameter type could be an Enum? or an Enum[] or an Enum?[] or an Enum[][]. You get it.
        // Let's just figure out what the base type of Enum actually is (int, long, byte, etc).
        Type baseEnumType = GetStrongEnumType(context.Parameter.Type);

        // Convert the argument to the base type of the enum. If this was invoked via slash commands,
        // Discord will send us the argument as a number, which STJ will convert to an unknown numeric type.
        // We need to ensure that the argument is the same type as it's enum base type.
        object? value = Convert.ChangeType(context.Argument, Enum.GetUnderlyingType(baseEnumType), CultureInfo.InvariantCulture);
        return value is not null && Enum.IsDefined(baseEnumType, value)
            ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(baseEnumType, value)))
            : Task.FromResult(Optional.FromNoValue<Enum>());
    }

    private static Type GetStrongEnumType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsArray)
        {
            // The type could be an array of enums or nullable
            // objects or worse: an array of arrays.
            return GetStrongEnumType(effectiveType.GetElementType()!);
        }

        return effectiveType;
    }
}

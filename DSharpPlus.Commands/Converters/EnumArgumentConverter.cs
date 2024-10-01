using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class EnumConverter : ISlashArgumentConverter<Enum>, ITextArgumentConverter<Enum>
{
    public DiscordApplicationCommandOptionType ParameterType =>
        DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Multiple Choice";
    public bool RequiresText => true;

    public Task<Optional<Enum>> ConvertAsync(ConverterContext context)
    {
        // The parameter type could be an Enum? or an Enum[] or an Enum?[] or an Enum[][]. You get it.
        Type enumType = IArgumentConverter.GetConverterFriendlyBaseType(context.Parameter.Type);
        if (context.Argument is string stringArgument)
        {
            return Task.FromResult(
                Enum.TryParse(enumType, stringArgument, true, out object? result)
                    ? Optional.FromValue((Enum)result)
                    : Optional.FromNoValue<Enum>()
            );
        }

        // Figure out what the base type of Enum actually is (int, long, byte, etc).
        Type baseEnumType = Enum.GetUnderlyingType(enumType);

        // Convert the argument to the base type of the enum. If this was invoked via slash commands,
        // Discord will send us the argument as a number, which STJ will convert to an unknown numeric type.
        // We need to ensure that the argument is the same type as it's enum base type.
        object? value = Convert.ChangeType(
            context.Argument,
            baseEnumType,
            CultureInfo.InvariantCulture
        );
        return value is not null && Enum.IsDefined(enumType, value)
            ? Task.FromResult(Optional.FromValue((Enum)Enum.ToObject(enumType, value)))
            : Task.FromResult(Optional.FromNoValue<Enum>());
    }
}

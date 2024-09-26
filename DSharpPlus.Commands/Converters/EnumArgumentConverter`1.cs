using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace DSharpPlus.Commands.Converters;

public class EnumConverter<T> : ISlashArgumentConverter<T>, ITextArgumentConverter<T> where T : struct, Enum
{
    private static readonly Type enumType = IArgumentConverter.GetConverterFriendlyBaseType(typeof(T));
    private static readonly Type baseEnumType = Enum.GetUnderlyingType(enumType);

    public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.Integer;
    public string ReadableName => "Multiple Choice";
    public bool RequiresText => true;

    public Task<Optional<T>> ConvertAsync(ConverterContext context)
    {
        // Null check for nullability warnings, however I think this is redundant as the base processor should handle this
        if (context.Argument is null)
        {
            return Task.FromResult(Optional.FromNoValue<T>());
        }
        else if (context.Argument is string stringArgument && Enum.TryParse(stringArgument, true, out T result))
        {
            return Task.FromResult(Optional.FromValue(result));
        }

        // Convert the argument to the base type of the enum. If this was invoked via slash commands,
        // Discord will send us the argument as a number, which STJ will convert to an unknown numeric type.
        // We need to ensure that the argument is the same type as it's enum base type.
        T value = context.Argument switch
        {
            byte b => TransformValue(b),
            sbyte sb => TransformValue(sb),
            short i16 => TransformValue(i16),
            ushort u16 => TransformValue(u16),
            int i32 => TransformValue(i32),
            uint u32 => TransformValue(u32),
            long i64 => TransformValue(i64),
            ulong u64 => TransformValue(u64),
            _ => throw new NotSupportedException($"DSharpPlus does not implement support for non-integer enums, found a value of type {context.Argument.GetType().FullName ?? context.Argument.GetType().Name}")
        };

        return Task.FromResult(Enum.IsDefined(value) ? Optional.FromValue(value) : Optional.FromNoValue<T>());
    }

    private static T TransformValue<TNumber>(TNumber number) where TNumber : unmanaged, IBinaryNumber<TNumber>
    {
        // ordered by relative likelihood for optimal codegen - Aki
        if (baseEnumType == typeof(int))
        {
            int value = (int)(object)number;
            return Unsafe.As<int, T>(ref value);
        }

        if (baseEnumType == typeof(uint))
        {
            uint value = (uint)(object)number;
            return Unsafe.As<uint, T>(ref value);
        }

        if (baseEnumType == typeof(ushort))
        {
            ushort value = (ushort)(object)number;
            return Unsafe.As<ushort, T>(ref value);
        }

        if (baseEnumType == typeof(byte))
        {
            byte value = (byte)(object)number;
            return Unsafe.As<byte, T>(ref value);
        }

        if (baseEnumType == typeof(short))
        {
            short value = (short)(object)number;
            return Unsafe.As<short, T>(ref value);
        }

        if (baseEnumType == typeof(long))
        {
            long value = (long)(object)number;
            return Unsafe.As<long, T>(ref value);
        }

        if (baseEnumType == typeof(ulong))
        {
            ulong value = (ulong)(object)number;
            return Unsafe.As<ulong, T>(ref value);
        }

        if (baseEnumType == typeof(sbyte))
        {
            sbyte value = (sbyte)(object)number;
            return Unsafe.As<sbyte, T>(ref value);
        }

        throw new NotSupportedException($"DSharpPlus does not implement support for non-integer enums, found enum with a base type of {baseEnumType.FullName ?? baseEnumType.Name}");
    }
}

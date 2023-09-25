using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Converts a string to an enum type.
    /// </summary>
    /// <typeparam name="T">Type of enum to convert.</typeparam>
    public class EnumConverter<T> : IArgumentConverter<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        Task<Optional<T>> IArgumentConverter<T>.ConvertAsync(string value, CommandContext ctx)
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsEnum)
                throw new InvalidOperationException("Cannot convert non-enum value to an enum.");

            return Enum.TryParse(value, !ctx.Config.CaseSensitive, out T ev)
                ? Task.FromResult(Optional.FromValue(ev))
                : Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}

using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class EnumConverter<T> : IArgumentConverter<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        Task<Optional<T>> IArgumentConverter<T>.ConvertAsync(string value, CommandContext ctx)
        {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsEnum)
                throw new InvalidOperationException("Cannot convert non-enum value to an enum.");

            if (Enum.TryParse(value, !ctx.Config.CaseSensitive, out T ev))
                return Task.FromResult(Optional.FromValue(ev));

            return Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}

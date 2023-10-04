using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class NullableConverter<T> : IArgumentConverter<Nullable<T>> where T : struct
    {
        async Task<Optional<Nullable<T>>> IArgumentConverter<Nullable<T>>.ConvertAsync(string value, CommandContext ctx)
        {
            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (value == "null")
                return Optional.FromValue<Nullable<T>>(null);

            if (ctx.CommandsNext.ArgumentConverters.TryGetValue(typeof(T), out var cv))
            {
                var cvx = (IArgumentConverter<T>)cv;
                var val = await cvx.ConvertAsync(value, ctx);
                return val.HasValue ? Optional.FromValue<Nullable<T>>(val.Value) : Optional.FromNoValue<Nullable<T>>();
            }

            return Optional.FromNoValue<Nullable<T>>();
        }
    }
}

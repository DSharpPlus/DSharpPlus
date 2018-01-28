using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    public class NullableConverter<T> : IArgumentConverter<Nullable<T>> where T : struct
    {
        public async Task<Optional<Nullable<T>>> ConvertAsync(string value, CommandContext ctx)
        {
            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (value == "null")
                return Optional<Nullable<T>>.FromValue(null);

            if (ctx.CommandsNext.ArgumentConverters.TryGetValue(typeof(T), out var cv))
            {
                var cvx = cv as IArgumentConverter<T>;
                var val = await cvx.ConvertAsync(value, ctx).ConfigureAwait(false);
                return val.HasValue ? Optional<Nullable<T>>.FromValue(val.Value) : Optional<Nullable<T>>.FromNoValue();
            }

            return Optional<Nullable<T>>.FromNoValue();
        }
    }
}

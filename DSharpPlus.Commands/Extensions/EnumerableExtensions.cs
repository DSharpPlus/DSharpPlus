using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Commands.Extensions;

internal static class EnumerableExtensions
{
    internal static TValue? SingleOrDefaultOfType<TValue, TEnumerable>(this IEnumerable<TEnumerable> enumerable)
        where TValue : TEnumerable
        => (TValue?)enumerable.SingleOrDefault(x => x.GetType() == typeof(TValue));
}

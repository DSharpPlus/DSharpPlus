using System;
using System.Collections.Generic;
using System.Linq;

namespace DSharpPlus.Commands.Extensions;

internal static class TypeExtensions
{
    public static bool IsValidVariadicParameterType(this Type type)
        => type.IsSZArray || type.GetInterfaces().Any(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
}

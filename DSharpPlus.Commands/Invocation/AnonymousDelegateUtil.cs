namespace DSharpPlus.Commands.Invocation;

using System;
using System.Reflection;
using System.Threading.Tasks;

/// <summary>
/// Contains stubs to invoke anonymous delegates
/// </summary>
internal static class AnonymousDelegateUtil
{
    public static Func<object?, object?[], ValueTask> GetAnonymousInvocationFunc(MethodInfo method, object? target)
    {
        if (method.ReturnType.IsAssignableTo(typeof(ValueTask)))
        {
            return async (object? _, object?[] parameters) => await (ValueTask)method.Invoke(target, parameters)!;
        }
        else if (method.ReturnType.IsAssignableTo(typeof(Task)))
        {
            return async (object? _, object?[] parameters) => await (Task)method.Invoke(target, parameters)!;
        }

        throw new InvalidOperationException
        (
            $"This command executor only supports ValueTask and Task return types for commands, found " +
            $"{method.ReturnType} on command method " +
            method.DeclaringType is not null ? $"{method.DeclaringType?.FullName ?? "<missing type>"}." : "" +
            method.Name
        );
    }
}

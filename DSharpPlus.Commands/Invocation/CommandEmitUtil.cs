namespace DSharpPlus.Commands.Invocation;

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

/// <summary>
/// Contains utilities to conveniently await all (supported) commands as ValueTasks.
/// </summary>
internal static class CommandEmitUtil
{
    /// <summary>
    /// Creates a wrapper function to invoke a command.
    /// </summary>
    /// <param name="method">The corresponding MethodInfo for this command.</param>
    /// <param name="target">The object targeted by this delegate, if applicable.</param>
    /// <exception cref="InvalidOperationException">Thrown if the command returns anything but ValueTask and Task.</exception>
    public static Func<object?, object?[], ValueTask> GetCommandInvocationFunc(MethodInfo method, object? target)
    {
        // This method is very likely to be an anonymous delegate, which happens to be very slow to invoke.
        // We're going to take the slow path here and simply do `MethodInfo.Invoke`, for lack of a better way.
        if (method.Name.Contains('<') && method.Name.Contains('>') && method.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
        {
            return AnonymousDelegateUtil.GetAnonymousInvocationFunc(method, target);
        }
        else if (method.ReturnType == typeof(ValueTask))
        {
            return GetValueTaskFunc(method);
        }
        else if (method.ReturnType == typeof(Task))
        {
            return GetTaskFunc(method);
        }

        // This could happen for `void` methods when the user explicitly builds a command tree with them.
        // We probably don't want to support `void` since the user will never be able to respond to the command...
        // Unless if it was done through context checks... Hm.
        throw new InvalidOperationException
        (
            $"This command executor only supports ValueTask and Task return types for commands, found " +
            $"{method.ReturnType} on command method " +
            method.DeclaringType is not null ? $"{method.DeclaringType?.FullName ?? "<missing type>"}." : "" +
            method.Name
        );
    }

    /// <summary>
    /// Emits a wrapper function around a command method that returns a <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="method">The method to wrap.</param>
    /// <returns>An asynchronous function that wraps the command method, returning a <see cref="ValueTask"/>.</returns>
    private static Func<object?, object?[], ValueTask> GetValueTaskFunc(MethodInfo method)
    {
        // Create the wrapper function
        DynamicMethod dynamicMethod = new($"{method.Name}-valuetask-wrapper", typeof(ValueTask), [typeof(object), typeof(object?[])]);

        // Create the wrapper logic
        EmitMethodWrapper(dynamicMethod.GetILGenerator(), method);

        // Return the delegate for the wrapper which invokes the method
        return dynamicMethod.CreateDelegate<Func<object?, object?[], ValueTask>>();
    }

    /// <summary>
    /// Emits a wrapper function around a command method that returns a <see cref="Task"/>.
    /// </summary>
    /// <param name="method">The method to wrap.</param>
    /// <returns>An asynchronous function that wraps the command method, returning a <see cref="ValueTask"/>.</returns>
    private static Func<object?, object?[], ValueTask> GetTaskFunc(MethodInfo method)
    {
        // Create the wrapper function
        DynamicMethod dynamicMethod = new($"{method.Name}-task-wrapper", typeof(Task), [typeof(object), typeof(object?[])]);

        // Create the wrapper logic
        EmitMethodWrapper(dynamicMethod.GetILGenerator(), method);

        // Return the delegate for the wrapper which invokes the method
        Func<object?, object?[], Task> taskWrapper = dynamicMethod.CreateDelegate<Func<object?, object?[], Task>>();

        // Create an async wrapper around the task wrapper
        return async (object? instance, object?[] parameters) => await taskWrapper(instance, parameters);
    }

    /// <summary>
    /// Writes the body of the wrapper function which invokes the command method.
    /// </summary>
    /// <param name="dynamicMethodIlGenerator"></param>
    /// <param name="method"></param>
    private static void EmitMethodWrapper(ILGenerator dynamicMethodIlGenerator, MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (!method.IsStatic)
        {
            // Load the instance (this) onto the stack.
            dynamicMethodIlGenerator.Emit(OpCodes.Ldarg_0);
            if (method.DeclaringType!.IsValueType)
            {
                // Change the handling behavior (unbox) of the instance (this) from a reference type to a value type.
                dynamicMethodIlGenerator.Emit(OpCodes.Unbox_Any, method.DeclaringType);
            }
        }

        // Load the arguments onto the stack.
        for (int i = 0; i < parameters.Length; i++)
        {
            // Ldarg_1 loads the array of arguments.
            dynamicMethodIlGenerator.Emit(OpCodes.Ldarg_1);

            // Ldc_I4 loads the index of the current argument.
            dynamicMethodIlGenerator.Emit(OpCodes.Ldc_I4, i);

            // Ldelem_Ref loads the element at the given index from the array.
            dynamicMethodIlGenerator.Emit(OpCodes.Ldelem_Ref);

            // Unbox value types or cast reference types.
            if (parameters[i].ParameterType.IsValueType)
            {
                // If the parameter is a value type, unbox it.
                // This is necessary because the argument is stored as an object (reference type)
                // when it needs to be treated as a value type.
                dynamicMethodIlGenerator.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
            }
            else
            {
                // If the parameter is a reference type, cast it
                // to the expected argument type for type safety.
                dynamicMethodIlGenerator.Emit(OpCodes.Castclass, parameters[i].ParameterType);
            }
        }

        // The method call is performed after loading the instance (if applicable)
        // and arguments onto the stack.
        dynamicMethodIlGenerator.Emit(OpCodes.Call, method);

        // Return from the method.
        dynamicMethodIlGenerator.Emit(OpCodes.Ret);
    }
}

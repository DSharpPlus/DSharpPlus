namespace DSharpPlus.Commands.Invocation;

using System;
using System.Reflection;
using System.Reflection.Emit;
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
    /// <exception cref="InvalidOperationException">Thrown if the command returns anything but ValueTask and Task.</exception>
    public static Func<object?, object?[], ValueTask> GetCommandInvocationFunc(MethodInfo method)
    {
        if (method.ReturnType == typeof(ValueTask))
        {
            return GetValueTaskFunc(method);
        }

        else if (method.ReturnType == typeof(Task))
        {
            return GetTaskFunc(method);
        }

        throw new InvalidOperationException
        (
            $"This command executor only supports ValueTask and Task return types for commands, found " +
            $"{method.ReturnType} on command method " +
            $"{(method.DeclaringType is not null ? $"{method.DeclaringType.FullName}." : "")}" +
            $"{method.Name}"
        );
    }

    // get a func for a command returning ValueTask - fairly trivial, just emits the wrapper
    private static Func<object?, object?[], ValueTask> GetValueTaskFunc(MethodInfo method)
    {
        DynamicMethod dynamicMethod = new
        (
            $"{method.Name}-valuetask-wrapper",
            typeof(ValueTask),
            [typeof(object), typeof(object?[])]
        );

        ILGenerator il = dynamicMethod.GetILGenerator();

        EmitMethodWrapper(il, method);

        return dynamicMethod.CreateDelegate<Func<object?, object?[], ValueTask>>();
    }

    // get a ValueTask func for a command returning Task, emitting an uniform Task wrapper and then
    // returning a lambda awaiting that Task wrapper
    private static Func<object?, object?[], ValueTask> GetTaskFunc(MethodInfo method)
    {
        DynamicMethod dynamicMethod = new
        (
            $"{method.Name}-task-wrapper",
            typeof(Task),
            [typeof(object), typeof(object?[])]
        );

        ILGenerator il = dynamicMethod.GetILGenerator();

        EmitMethodWrapper(il, method);

        Func<object?, object?[], Task> taskWrapper = dynamicMethod.CreateDelegate<Func<object?, object?[], Task>>();

        return async (object? instance, object?[] parameters) => await taskWrapper(instance, parameters);
    }

    // the heavy lifting of emitting IL actually just works entirely the same for both, since we don't do
    // anything further with the returned Task/ValueTask, and our wrapper is synchronous.
    private static void EmitMethodWrapper(ILGenerator il, MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        ConstructorInfo argumentExceptionCtor = typeof(ArgumentException).GetConstructor([typeof(string), typeof(string)])!;
        Label validation = il.DefineLabel();

        // first, verify validity of the parameter array by length. this makes further issues easier to
        // diagnose, since we'll be getting the actual error when we expect to.
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldlen);
        il.Emit(OpCodes.Conv_I4);
        il.Emit(OpCodes.Ldc_I4, parameters.Length);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ceq);
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloc_0);

        // branch off if the check was successful
        il.Emit(OpCodes.Brfalse_S, validation);

        // throw if unsuccessful
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Ldstr, "args");
        il.Emit(OpCodes.Ldstr, "Incorrect number of arguments provided.");
        il.Emit(OpCodes.Newobj, argumentExceptionCtor);
        il.Emit(OpCodes.Throw);

        // cast and unbox the arguments
        il.MarkLabel(validation);

        if (!method.IsStatic)
        {
            il.Emit(OpCodes.Ldarg_0);

            // not static implies declared within a type
            if (method.DeclaringType!.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, method.DeclaringType!);
            }
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);

            // unbox value types, cast reference types
            if (parameters[i].ParameterType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, parameters[i].ParameterType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, parameters[i].ParameterType);
            }
        }

        // actually call the command method
        il.Emit(OpCodes.Call, method);

        il.Emit(OpCodes.Ret);
    }
}

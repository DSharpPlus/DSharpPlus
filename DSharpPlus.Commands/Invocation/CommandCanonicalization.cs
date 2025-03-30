using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands.Invocation;

/// <summary>
/// Contains utilities to conveniently await all (supported) commands as ValueTasks.
/// </summary>
internal static class CommandCanonicalization
{
    /// <summary>
    /// Creates a wrapper function to invoke a command.
    /// </summary>
    /// <param name="method">The corresponding MethodInfo for this command.</param>
    /// <param name="name">The command name for inclusion in debug info.</param>
    /// <exception cref="InvalidOperationException">Thrown if the command returns anything but ValueTask and Task.</exception>
    public static Func<CommandContext, object?[], IServiceProvider, ValueTask> GetCommandInvocationFunc(MethodInfo method, string name)
    {
        if (method.ReturnType == typeof(ValueTask))
        {
            return GetValueTaskFunc(method, name);
        }
        else if (method.ReturnType == typeof(Task))
        {
            return GetTaskFunc(method, name);
        }

        // This could happen for `void` methods when the user explicitly builds a command tree with them.
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
    /// <param name="name">The name of the command, used for better error logging.</param>
    /// <returns>An asynchronous function that wraps the command method, returning a <see cref="ValueTask"/>.</returns>
    private static Func<CommandContext, object?[], IServiceProvider, ValueTask> GetValueTaskFunc(MethodInfo method, string name)
    {
        DynamicMethod dynamicMethod = new($"command-{name}-wrapper", typeof(ValueTask), [typeof(CommandContext), typeof(object?[]), typeof(IServiceProvider)]);
        EmitMethodWrapper(dynamicMethod.GetILGenerator(), method);
        return dynamicMethod.CreateDelegate<Func<CommandContext, object?[], IServiceProvider, ValueTask>>();
    }

    /// <summary>
    /// Emits a wrapper function around a command method that returns a <see cref="Task"/>.
    /// </summary>
    /// <param name="method">The method to wrap.</param>
    /// <param name="name">The name of the command, used for better error logging.</param>
    /// <returns>An asynchronous function that wraps the command method, returning a <see cref="ValueTask"/>.</returns>
    private static Func<CommandContext, object?[], IServiceProvider, ValueTask> GetTaskFunc(MethodInfo method, string name)
    {
        DynamicMethod dynamicMethod = new($"command-{name}-task-wrapper", typeof(Task), [typeof(CommandContext), typeof(object?[]), typeof(IServiceProvider)]);
        EmitMethodWrapper(dynamicMethod.GetILGenerator(), method);
        Func<CommandContext, object?[], IServiceProvider, ValueTask> taskWrapper = dynamicMethod.CreateDelegate<Func<CommandContext, object?[], IServiceProvider, ValueTask>>();

        return async (context, parameters, services) => await taskWrapper(context, parameters, services);
    }

    /// <summary>
    /// Writes the body of the wrapper function which invokes the command method.
    /// </summary>
    private static void EmitMethodWrapper(ILGenerator il, MethodInfo method)
    {
        // if the method isn't static, we need to get an instance from the service provider and onto the evaluation stack
        if (!method.IsStatic)
        {
            Type serviceProviderExtensions = typeof(ServiceProviderServiceExtensions);

            MethodInfo getTypeFromHandle = typeof(Type).GetMethod
            (
                nameof(Type.GetTypeFromHandle),
                BindingFlags.Public | BindingFlags.Static,
                [typeof(RuntimeTypeHandle)]
            )!;

            MethodInfo getService = serviceProviderExtensions.GetMethod
            (
                nameof(ServiceProviderServiceExtensions.GetRequiredService),
                BindingFlags.Public | BindingFlags.Static,
                [typeof(IServiceProvider), typeof(Type)]
            )!;

            // load the service provider (because it's the first argument, that way we don't have to shuffle the stack), then the declaring
            // type as a type handle, get the type object from the handle, then call GetRequiredService. sounds complicated? it is. here's
            // an explanation of the evaluation stack:
            // |----------------------------------------------------------------------------------------------------------------------------|
            // | start: evaluation stack is empty                                                                                           |
            // |----------------------------------------------------------------------------------------------------------------------------|
            // | ldarg.2 loads the service provider, so our stack is now, top to bottom:                                                    |
            // |                                                                                                                            |
            // | class System.IServiceProvider                                                                                              |
            // |----------------------------------------------------------------------------------------------------------------------------|
            // | ldtoken loads the declaring type, so our stack is now, top to bottom:                                                      |
            // |                                                                                                                            |
            // | valuetype System.RuntimeTypeHandle                                                                                         |
            // | class System.IServiceProvider                                                                                              |
            // |----------------------------------------------------------------------------------------------------------------------------|
            // | call System.Type:GetTypeFromHandle(...) *consumes* the RuntimeTypeHandle as an argument and instead pushes the Type        |
            // | object from its return value onto the stack, so our stack is now, top to bottom:                                           |
            // |                                                                                                                            |
            // | class System.Type                                                                                                          |
            // | class System.IServiceProvider                                                                                              |
            // |----------------------------------------------------------------------------------------------------------------------------|
            // | call Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions:GetRequiredService(...) consumes both       |
            // | locals on the stack as arguments and pushes the returned instance from its return value onto the stack, so our stack is    |
            // | now, top to bottom:                                                                                                        |
            // |                                                                                                                            |
            // | class System.Object                                                                                                        |
            // |                                                                                                                            |
            // | and that object is exactly what we need to invoke the command on (if the command is static, we won't ever emit this code)  |
            // | so we're now ready to moving on to loading args, which fortunately is a whole lot simpler to do.                           |
            // |----------------------------------------------------------------------------------------------------------------------------|
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldtoken, method.DeclaringType!);
            il.Emit(OpCodes.Call, getTypeFromHandle);
            il.Emit(OpCodes.Call, getService);
        }

        // the first arg is the command context, which we'll always have to load
        il.Emit(OpCodes.Ldarg_0);

        // loads each argument from the object? array provided and unboxes it. i spent way too much effort on the table above so i'm going
        // to make another one here, so that this effort doesn't all go to waste:
        // |----------------------------------------------------------------------------------------------------------------------------|
        // | start: from here on out we may or may not have a class System.Object at the bottom of the stack to invoke the command on.  |
        // | this explanation assumes we do, but just pretend it's not there for static commands. our stack is, top to bottom:          |
        // |                                                                                                                            |
        // | class DSharpPlus.Commands.CommandContext                                                                                   |
        // | class System.Object                                                                                                        |
        // |----------------------------------------------------------------------------------------------------------------------------|
        // | ldarg.1 loads the parameter array, so our stack is now, top to bottom:                                                     |
        // |                                                                                                                            |
        // | class System.Object[]                                                                                                      |
        // | class DSharpPlus.Commands.CommandContext                                                                                   |
        // | class System.Object                                                                                                        |
        // |----------------------------------------------------------------------------------------------------------------------------|
        // | ldc.i4 loads the array index of our parameter as a constant (i4 = 4 byte integer), so our stack is now, top to bottom:     |
        // |                                                                                                                            |
        // | valuetype System.Int32                                                                                                     |
        // | class System.Object[]                                                                                                      |
        // | class DSharpPlus.Commands.CommandContext                                                                                   |
        // | class System.Object                                                                                                        |
        // |----------------------------------------------------------------------------------------------------------------------------|
        // | ldelem.ref loads the element of the array at the index indicated by the constant, pops both of those from the evaluation   |
        // | stack and pushes the loaded element instead; so our stack is now, top to bottom:                                           |
        // |                                                                                                                            |
        // | class System.Object                                                                                                        |
        // | class DSharpPlus.Commands.CommandContext                                                                                   |
        // | class System.Object                                                                                                        |
        // |----------------------------------------------------------------------------------------------------------------------------|
        // | unbox.any, when used on a boxed value type, unboxes it (which is necessary), and when used on a reference type, acts like  |
        // | castclass (which is nice, but not necessary). so assuming our argument type here is System.Int64, our stack is now,        |
        // | top to bottom:                                                                                                             |
        // |                                                                                                                            |
        // | valuetype System.Int64                                                                                                     |
        // | class DSharpPlus.Commands.CommandContext                                                                                   |
        // | class System.Object                                                                                                        |
        // |                                                                                                                            |
        // | from here on we repeat this sequence for each argument, with each argument ending up on top of the previous one; and all   |
        // | of them on top of CommandContext, so when we eventually call the command, all arguments to the method, starting with       |
        // | CommandContext, are unfurled and ordered correctly for the final call instruction.                                         |
        // |----------------------------------------------------------------------------------------------------------------------------|
        ParameterInfo[] parameters = method.GetParameters();

        for (int i = 0; i < parameters.Length - 1; i++)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Unbox_Any, parameters[i + 1].ParameterType);
        }

        // after setting the stack up, this call will destroy all arguments as well as the instance we called it on, and instead push
        // the Task/ValueTask onto the stack, which we return to be awaited. this code doesn't at all differ between using Task and
        // ValueTask, no special-casing needed.
        il.Emit(OpCodes.Call, method);
        il.Emit(OpCodes.Ret);
    }
}

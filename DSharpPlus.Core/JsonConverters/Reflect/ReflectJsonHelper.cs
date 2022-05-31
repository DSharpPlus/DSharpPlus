using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace DSharpPlus.Core.JsonConverters.Reflect
{
    /// <summary>
    /// Delegate type used to write a property of an instance of type <typeparamref name="T"/> to JSON.
    /// </summary>
    /// <typeparam name="T"> The type of the instance. </typeparam>
    /// <param name="writer"> The active JSON writer. </param>
    /// <param name="instance"> The instance to read from. </param>
    /// <param name="options"> The active JSON serializer options. </param>
    internal delegate void WriteProp<T>(Utf8JsonWriter writer, T instance, JsonSerializerOptions options);

    /// <summary>
    /// Delegate type used to read a JSON value of a property into an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"> The type of the instance. </typeparam>
    /// <param name="reader"> The active JSON reader. </param>
    /// <param name="instance"> The instance to modify. </param>
    /// <param name="options"> The active JSON serializer options. </param>
    internal delegate void ReadProp<T>(ref Utf8JsonReader reader, T instance, JsonSerializerOptions options);

    /// <summary>
    /// Helper class used by <see cref="ReflectJsonConverter{T}"/> for reflection.
    /// </summary>
    internal static class ReflectJsonHelper
    {
        internal static WriteProp<T> CompileWrite<T>(PropertyInfo property)
        {
            // (writer, instance, options) => JsonSerializer.Serialize(writer, instance.<property>, options)

            DynamicMethod method = CreateDynamicMethod(
                name: $"Write:{typeof(T).Name}.{property.Name}",
                parameterTypes: new[] { typeof(Utf8JsonWriter), typeof(T), typeof(JsonSerializerOptions) },
                returnType: typeof(void));

            MethodInfo writeMethod = typeof(ReflectJsonHelper)
                .GetMethod(nameof(Write), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(property.PropertyType);

            ILGenerator il = method.GetILGenerator();

            // Push writer
            il.Emit(OpCodes.Ldarg_0);

            // Push instance.<property>
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, property.GetMethod!);

            // Push options
            il.Emit(OpCodes.Ldarg_2);

            // JsonSerializer.Serialize(writer, instance.<property>, options)
            il.Emit(OpCodes.Call, writeMethod);
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<WriteProp<T>>();
        }

        internal static ReadProp<T> CompileRead<T>(PropertyInfo property)
        {
            // (ref reader, instance, options) => instance.<property> = JsonSerializer.Deserialize(ref reader, options)

            DynamicMethod method = CreateDynamicMethod(
                name: $"Read:{typeof(T).Name}.{property.Name}",
                parameterTypes: new[] { typeof(Utf8JsonReader).MakeByRefType(), typeof(T), typeof(JsonSerializerOptions) },
                returnType: typeof(void));

            MethodInfo readMethod = typeof(ReflectJsonHelper)
                .GetMethod(nameof(Read), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(property.PropertyType);

            ILGenerator il = method.GetILGenerator();

            // Push instance to the stack so it is the first value after the next call
            il.Emit(OpCodes.Ldarg_1);

            // JsonSerializer.Deserialize(ref reader, options) result on the stack
            il.Emit(OpCodes.Ldarg_0); // Already a by-ref
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, readMethod);

            // instance = result of above
            il.Emit(OpCodes.Callvirt, property.SetMethod!);
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<ReadProp<T>>();
        }

        internal static Func<T, JsonFieldState> CompileGetFieldState<T>(PropertyInfo property)
        {
            // instance => {
            //   JsonFieldState result = 0;
            //   if (ECDEquals(instance.<property>, default(<property>)))
            //     result |= JsonFieldState.Default;
            //   return result;
            // }

            DynamicMethod method = CreateDynamicMethod(
                name: $"GetFieldState:{typeof(T).Name}.{property.Name}",
                parameterTypes: new[] { typeof(T) },
                returnType: typeof(JsonFieldState));

            MethodInfo equalsMethod = typeof(ReflectJsonHelper)
                .GetMethod(nameof(ECDEquals), BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(property.PropertyType);

            ILGenerator il = method.GetILGenerator();

            LocalBuilder resultVar = il.DeclareLocal(typeof(JsonFieldState));
            LocalBuilder defaultVar = il.DeclareLocal(property.PropertyType);

            // defaultVar holds default(TProp)
            il.Emit(OpCodes.Ldloca, defaultVar);
            il.Emit(OpCodes.Initobj, property.PropertyType);

            // result = 0
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc, resultVar);

            // ECDEquals(instance.<property>, defaultVar)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, property.GetMethod!);
            il.Emit(OpCodes.Ldloc, defaultVar);
            il.Emit(OpCodes.Call, equalsMethod);

            Label endOfIf = il.DefineLabel();
            // if (...)
            il.Emit(OpCodes.Brfalse, endOfIf);

            // Body of if begin
            int resultIfDefault = property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null
                ? (int)JsonFieldState.Default
                : (int)(JsonFieldState.Null | JsonFieldState.Default);

            // result = JsonFieldState.Null | JsonFieldState.Default
            il.Emit(OpCodes.Ldc_I4, resultIfDefault);
            il.Emit(OpCodes.Stloc, resultVar);

            // Body of if end
            il.MarkLabel(endOfIf);

            // return result;
            il.Emit(OpCodes.Ldloc, resultVar);
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<Func<T, JsonFieldState>>();
        }

        internal static Func<T> CompileConstructor<T>()
        {
            // () => new T()

            ConstructorInfo? ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new InvalidOperationException($"{typeof(T)} does not have a suitable default constructor.");
            }

            DynamicMethod method = CreateDynamicMethod(
                name: $"Construct:{typeof(T).Name}",
                parameterTypes: Type.EmptyTypes,
                returnType: typeof(T));

            ILGenerator il = method.GetILGenerator();

            il.Emit(OpCodes.Newobj, ctor);
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<Func<T>>();
        }

        private static DynamicMethod CreateDynamicMethod(string name, Type[] parameterTypes, Type returnType)
            => new(
                name: name,
                attributes: MethodAttributes.Public | MethodAttributes.Static,
                callingConvention: CallingConventions.Standard,
                returnType: returnType,
                parameterTypes: parameterTypes,
                m: typeof(ReflectJsonHelper).Module,
                skipVisibility: true);

        // Helper methods used to avoid ambiguity when trying to find certain methods with reflection

        internal static void Write<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, options);

        internal static T? Read<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<T>(ref reader, options);

        internal static bool ECDEquals<T>(T? t1, T? t2)
            => EqualityComparer<T>.Default.Equals(t1, t2);
    }
}

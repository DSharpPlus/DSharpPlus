using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.Commands.Processors;

public abstract partial class BaseCommandProcessor<TConverter, TConverterContext, TCommandContext> : ICommandProcessor
    where TConverter : class, IArgumentConverter
    where TConverterContext : ConverterContext
    where TCommandContext : CommandContext
{
    /// <summary>
    /// A factory used for creating and caching converter objects and delegates.
    /// </summary>
    protected class ConverterDelegateFactory
    {
        private static readonly MethodInfo createConverterDelegateMethod = typeof(ConverterDelegateFactory)
            .GetMethod(nameof(CreateConverterDelegate), BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new UnreachableException($"Method {nameof(CreateConverterDelegate)} was unable to be found.");

        /// <summary>
        /// The converter instance, if it's already been created.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="GetConverter(IServiceProvider)"/> to get the converter instance.
        /// </remarks>
        public TConverter? ConverterInstance { get; private set; }

        /// <summary>
        /// The converter's type (not to be confused with parameter type). Only available when passing the type to the constructor.
        /// </summary>
        public Type? ConverterType { get; private set; }

        /// <summary>
        /// The parameter type that this converter converts to.
        /// </summary>
        public Type ParameterType { get; init; }

        /// <summary>
        /// The command processor that this converter is associated with.
        /// </summary>
        public BaseCommandProcessor<TConverter, TConverterContext, TCommandContext> CommandProcessor { get; init; }

        /// <summary>
        /// The delegate that executes the converter, casting the returned strongly typed value (<see cref="Optional{T}"/>)
        /// to a less strongly typed value (<see cref="IOptional"/>) for easier argument converter invocation.
        /// </summary>
        private ConverterDelegate? converterDelegate;

        /// <summary>
        /// Creates a new converter delegate factory, which will use
        /// the provided converter instance to create the delegate.
        /// </summary>
        /// <param name="processor">The command processor that this converter is associated with.</param>
        /// <param name="parameterType">The parameter type that this converter converts to.</param>
        /// <param name="converter">The converter instance to use.</param>
        public ConverterDelegateFactory(BaseCommandProcessor<TConverter, TConverterContext, TCommandContext> processor, Type parameterType, TConverter converter)
        {
            this.ConverterInstance = converter;
            this.ConverterType = null;
            this.ParameterType = parameterType;
            this.CommandProcessor = processor;
        }

        /// <summary>
        /// Creates a new converter delegate factory, which will obtain or construct
        /// <see cref="ConverterInstance"/> through the service provider as needed.
        /// The converter delegate will be created using the newly created converter instance.
        /// </summary>
        public ConverterDelegateFactory(BaseCommandProcessor<TConverter, TConverterContext, TCommandContext> processor, Type parameterType, Type converterType)
        {
            this.ConverterType = converterType;
            this.ParameterType = parameterType;
            this.CommandProcessor = processor;
        }

        /// <summary>
        /// Creates and caches the converter instance if it hasn't been created yet.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for creating the converter instance, if needed.</param>
        /// <returns>The converter instance.</returns>
        [MemberNotNull(nameof(ConverterInstance))]
        public TConverter GetConverter(IServiceProvider serviceProvider)
        {
            if (this.ConverterInstance is not null)
            {
                return this.ConverterInstance;
            }
            else if (this.ConverterType is null)
            {
                throw new UnreachableException($"Both {nameof(this.ConverterInstance)} and {nameof(this.ConverterType)} are null. Please open an issue about this.");
            }

            this.ConverterInstance = (TConverter)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, this.ConverterType);
            return this.ConverterInstance;
        }

        /// <summary>
        /// Creates and caches the converter delegate if it hasn't been created yet.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use for creating the converter instance, if needed.</param>
        /// <returns>The converter delegate.</returns>
        [MemberNotNull(nameof(converterDelegate))]
        public ConverterDelegate GetConverterDelegate(IServiceProvider serviceProvider)
        {
            if (this.converterDelegate is not null)
            {
                return this.converterDelegate;
            }

            // Sets the converter instance if it's null
            GetConverter(serviceProvider);

            // Create the generic version of CreateConverterDelegate<T> to the parameter type
            MethodInfo createConverterDelegateGenericMethod = createConverterDelegateMethod.MakeGenericMethod(this.ParameterType);

            // Invoke the generic method to obtain ExecuteConverterAsync<T> as a delegate
            this.converterDelegate = (ConverterDelegate)createConverterDelegateGenericMethod.Invoke(this, [])!;
            return this.converterDelegate;
        }

        /// <summary>
        /// A generic method used for obtaining the <see cref="ExecuteConverterAsync{T}"/> method as a delegate.
        /// </summary>
        /// <typeparam name="T">The type of the parameter that the converter converts to.</typeparam>
        /// <returns>A delegate that executes the converter, casting the returned strongly typed value (<see cref="Optional{T}"/>)
        /// to a less strongly typed value (<see cref="IOptional"/>) for easier argument converter invocation.
        /// </returns>
        private ConverterDelegate CreateConverterDelegate<T>() => ((Delegate)ExecuteConverterAsync<T>).Method.CreateDelegate<ConverterDelegate>(this);

        /// <summary>
        /// Invokes the converter on the provided context, casting the returned strongly typed value (<see cref="Optional{T}"/>)
        /// to a less strongly typed value (<see cref="IOptional"/>) for easier argument converter invocation.
        /// </summary>
        /// <param name="context">The converter context passed to the converter.</param>
        /// <typeparam name="T">The type of the parameter that the converter converts to.</typeparam>
        /// <returns>The result of the converter.</returns>
        private async ValueTask<IOptional> ExecuteConverterAsync<T>(ConverterContext context) => await this.CommandProcessor.ExecuteConverterAsync<T>(
            this.ConverterInstance!,
            context.As<TConverterContext>()
        );

        /// <inheritdoc/>
        public override string? ToString()
        {
            if (this.ConverterType is not null)
            {
                return this.ConverterType.FullName ?? this.ConverterType.Name;
            }
            else if (this.ConverterInstance is not null)
            {
                Type type = this.ConverterInstance.GetType();
                return type.FullName ?? type.Name;
            }
            else if (this.converterDelegate is not null)
            {
                return this.converterDelegate.Method.DeclaringType is null
                    ? this.converterDelegate.ToString()
                    : this.converterDelegate.Method.DeclaringType.FullName
                        ?? this.converterDelegate.Method.DeclaringType.Name;
            }

            return base.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is ConverterDelegateFactory other
            && (this.ConverterType == other.ConverterType
                || this.ConverterInstance == other.ConverterInstance
                || this.converterDelegate == other.converterDelegate);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(this.ConverterType, this.ConverterInstance, this.converterDelegate);

        /// <inheritdoc/>
        public static bool operator ==(ConverterDelegateFactory left, ConverterDelegateFactory right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ConverterDelegateFactory left, ConverterDelegateFactory right) => !left.Equals(right);
    }
}

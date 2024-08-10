using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Processors;

/// <summary>
/// A command processor containing command logic that's shared between all command processors.
/// </summary>
/// <remarks>
/// When implementing a new command processor, it's recommended to inherit from this class.
/// You can however implement <see cref="ICommandProcessor"/> directly instead, if desired.
/// </remarks>
/// <typeparam name="TConverter">
/// The converter type that's associated with this command processor.
/// May have extra metadata related to this processor specifically.
/// </typeparam>
/// <typeparam name="TConverterContext">The context type that's used for argument converters.</typeparam>
/// <typeparam name="TCommandContext">The context type that's used for command execution.</typeparam>
public abstract partial class BaseCommandProcessor<TConverter, TConverterContext, TCommandContext> : ICommandProcessor
    where TConverter : class, IArgumentConverter
    where TConverterContext : ConverterContext
    where TCommandContext : CommandContext
{
    /// <inheritdoc />
    public Type ContextType => typeof(TCommandContext);

    /// <inheritdoc />
    public abstract IReadOnlyList<Command> Commands { get; }

    /// <inheritdoc cref="ICommandProcessor.Converters" />
    public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } = new Dictionary<Type, TConverter>();

    /// <inheritdoc />
    IReadOnlyDictionary<Type, IArgumentConverter> ICommandProcessor.Converters => Unsafe.As<IReadOnlyDictionary<Type, IArgumentConverter>>(this.Converters);

    /// <summary>
    /// A dictionary of argument converter delegates indexed by the output type they convert to.
    /// </summary>
    public IReadOnlyDictionary<Type, ConverterDelegate> ConverterDelegates { get; protected set; } = new Dictionary<Type, ConverterDelegate>();

    /// <summary>
    /// A dictionary of argument converter factories indexed by the output type they convert to.
    /// These factories populate the <see cref="Converters"/> and <see cref="ConverterDelegates"/> dictionaries.
    /// </summary>
    protected Dictionary<Type, ConverterDelegateFactory> converterFactories = [];

    /// <summary>
    /// The extension this processor belongs to.
    /// </summary>
    protected CommandsExtension? extension;

    /// <summary>
    /// The logger for this processor.
    /// </summary>
    protected ILogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>> logger = NullLogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>>.Instance;

    /// <summary>
    /// Registers a new argument converter with the processor.
    /// </summary>
    /// <param name="converter">The converter to register.</param>
    /// <typeparam name="T">The type that the converter converts to.</typeparam>
    public virtual void AddConverter<T>(TConverter converter) => AddConverter(typeof(T), converter);

    /// <summary>
    /// Registers a new argument converter with the processor.
    /// </summary>
    /// <param name="type">The type that the converter converts to.</param>
    /// <param name="converter">The converter to register.</param>
    public virtual void AddConverter(Type type, TConverter converter) => AddConverter(new(this, type, converter));

    /// <summary>
    /// Scans the specified assembly for argument converters and registers them with the processor.
    /// The argument converters will be created through the <see cref="IServiceProvider"/> provided to the <see cref="CommandsExtension"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan for argument converters.</param>
    public virtual void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());

    /// <summary>
    /// Adds multiple argument converters to the processor.
    /// </summary>
    /// <remarks>
    /// This method WILL NOT THROW if a converter is invalid. Instead, it will log an error and continue.
    /// </remarks>
    /// <param name="types">The types to add as argument converters.</param>
    public virtual void AddConverters(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            // Ignore types that don't have a concrete implementation (abstract classes or interfaces)
            // Additionally ignore types that have open generics (IArgumentConverter<T>)
            // instead of closed generics (IArgumentConverter<string>)
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition || !type.IsAssignableTo(typeof(TConverter)))
            {
                continue;
            }

            // Check if the type implements IArgumentConverter<T>
            Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<>));
            if (genericArgumentConverter is null)
            {
                BaseCommandLogging.invalidArgumentConverterImplementation(
                    this.logger,
                    type.FullName ?? type.Name,
                    typeof(IArgumentConverter<>).FullName ?? typeof(IArgumentConverter<>).Name,
                null);

                continue;
            }

            // GenericTypeArguments[0] here is the T in IArgumentConverter<T>
            AddConverter(new(this, genericArgumentConverter.GenericTypeArguments[0], type));
        }
    }

    /// <summary>
    /// Registers a new argument converter factory with the processor.
    /// </summary>
    /// <param name="factory">The factory that will create the argument converter and it's delegate.</param>
    protected virtual void AddConverter(ConverterDelegateFactory factory)
    {
        if (!this.converterFactories.TryGetValue(factory.ParameterType, out ConverterDelegateFactory? existingFactory))
        {
            BaseCommandLogging.duplicateArgumentConvertersRegistered(
                this.logger,
                factory.ConverterType?.GetType().FullName ?? factory.ConverterInstance!.GetType().FullName!,
                factory.ParameterType.FullName ?? factory.ParameterType.Name,
                existingFactory?.ConverterType?.FullName ?? existingFactory?.ConverterInstance?.GetType().FullName!,
                null
            );

            return;
        }

        this.converterFactories.Add(factory.ParameterType, factory);
    }

    /// <inheritdoc />
    [MemberNotNull(nameof(extension))]
    public virtual ValueTask ConfigureAsync(CommandsExtension extension)
    {
        this.extension = extension;
        this.logger = extension.ServiceProvider.GetService<ILogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>>>() ?? NullLogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>>.Instance;

        // Register all converters from the processor's assembly
        AddConverters(GetType().Assembly);

        // Populate the default converters
        Dictionary<Type, TConverter> converters = [];
        Dictionary<Type, ConverterDelegate> converterDelegates = [];
        foreach (KeyValuePair<Type, ConverterDelegateFactory> factory in this.converterFactories)
        {
            converters.Add(factory.Key, factory.Value.GetConverter(extension.ServiceProvider));
            converterDelegates.Add(factory.Key, factory.Value.GetConverterDelegate(extension.ServiceProvider));
        }

        this.Converters = converters.ToFrozenDictionary();
        this.ConverterDelegates = converterDelegates.ToFrozenDictionary();
        return default;
    }

    /// <summary>
    /// Finds the base type to use for converter registration.
    /// </summary>
    /// <remarks>
    /// More specifically, this methods returns the base type that can be found from <see cref="Nullable{T}"/>, <see cref="Enum"/>, or <see cref="Array"/>'s.
    /// </remarks>
    /// <param name="type">The type to find the base type for.</param>
    /// <returns>The base type to use for converter registration.</returns>
    public static Type GetConverterFriendlyBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;
        if (effectiveType.IsEnum)
        {
            return typeof(Enum);
        }
        else if (effectiveType.IsArray)
        {
            // The type could be an array of enums or nullable
            // objects or worse: an array of arrays.
            return GetConverterFriendlyBaseType(effectiveType.GetElementType()!);
        }

        return effectiveType;
    }

    /// <summary>
    /// Parses the arguments provided to the command and returns a prepared command context.
    /// </summary>
    /// <param name="converterContext">The context used for the argument converters.</param>
    /// <returns>The prepared CommandContext.</returns>
    public virtual async ValueTask<IReadOnlyDictionary<CommandParameter, object?>> ParseParametersAsync(TConverterContext converterContext)
    {
        // Populate the parsed arguments with <see cref="ArgumentNotParsedValue"/>
        // to indicate that the arguments haven't been parsed yet.
        // If this method ever exits early without finishing parsing, the
        // callee will know where the argument parsing stopped.
        Dictionary<CommandParameter, object?> parsedArguments = new(converterContext.Command.Parameters.Count);
        foreach (CommandParameter parameter in converterContext.Command.Parameters)
        {
            parsedArguments.Add(parameter, new ArgumentNotParsedValue());
        }

        try
        {
            while (converterContext.NextParameter())
            {
                object? parsedArgument = await ParseParameterAsync(converterContext);
                if (parsedArgument is ArgumentNotParsedValue)
                {
                    // Try to fill with it's default value
                    if (!converterContext.Parameter.DefaultValue.HasValue)
                    {
                        continue;
                    }

                    parsedArgument = converterContext.Parameter.DefaultValue.Value;
                }

                parsedArguments[converterContext.Parameter] = parsedArgument;
                if (parsedArgument is ArgumentFailedConversionValue)
                {
                    // Stop parsing if the argument failed to convert.
                    // The other parameters will be set to <see cref="ArgumentNotParsedValue"/>.
                    // ...XML docs don't work in comments. Pretend they do <3
                    return parsedArguments;
                }
            }
        }
        catch (Exception error)
        {
            // If an exception occurs during argument parsing, parsing is immediately stopped.
            // We'll set the current parameter to an error state and return the parsed arguments.
            // The callee will choose how to handle the error.
            parsedArguments[converterContext.Parameter] = new ArgumentFailedConversionValue
            {
                Error = error
            };
        }

        return parsedArguments;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="converterContext"></param>
    public virtual async ValueTask<object?> ParseParameterAsync(TConverterContext converterContext)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("The processor has not been configured yet.");
        }
        else if (converterContext.Argument is null)
        {
            return new ArgumentNotParsedValue();
        }

        ConverterDelegate converterDelegate = this.ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Parameter.Type)];

        // Multi-argument parameters are handled differently
        // than regular parameters. They're treated as a single
        // parameter with multiple values.
        MultiArgumentAttribute? multiArgumentAttribute = converterContext.Parameter.Attributes.FirstOrDefault(attribute => attribute is MultiArgumentAttribute) as MultiArgumentAttribute;

        // Check if the parameter is using params instead of a multi-argument attribute.
        if (multiArgumentAttribute is null && converterContext.Parameter.Attributes.Any(attribute => attribute is ParamArrayAttribute))
        {
            multiArgumentAttribute = new MultiArgumentAttribute(int.MaxValue, 1);
        }

        // If the parameter is a multi-argument parameter or params, we'll
        // parse all the arguments until we reach the maximum argument count.
        if (multiArgumentAttribute is not null)
        {
            List<object?> multiArgumentValues = new(multiArgumentAttribute.MaximumArgumentCount);
            while (converterContext.NextArgument())
            {
                IOptional? parsedArgument = await converterDelegate(converterContext);
                if (!parsedArgument.HasValue)
                {
                    // If the argument converter failed, we might
                    // have reached the end of the arguments.
                    // Return what we have now, the next time this
                    // method is invoked, we'll be able to determine if
                    // the argument failed to convert or if there are
                    // no more arguments for this parameter.
                    return new ArgumentFailedConversionValue();
                }

                multiArgumentValues.Add(parsedArgument);
            }

            if (multiArgumentValues.Count < multiArgumentAttribute.MinimumArgumentCount)
            {
                // If the minimum argument count isn't met, we'll return an error.
                // The callee will choose how to handle the error.
                return new ArgumentFailedConversionValue();
            }

            return multiArgumentValues;
        }

        // Just a regular parameter, parse it like normal.
        IOptional optional = await converterDelegate(converterContext);
        if (optional.HasValue)
        {
            // Thanks Roslyn for not yelling at me to make a ternary operator.
            return optional.RawValue;
        }

        // If there's invalid input, the argument converter should throw.
        // Returning an Optional<T> with no value means that the argument converter
        // expected to fail and everything is working as expected.
        // We return a special value here to indicate that the argument failed to convert,
        // which allows the callee to choose how to handle the failure
        // (e.g. return an error message or selecting the default value).
        return new ArgumentFailedConversionValue();
    }

    /// <summary>
    /// Executes an argument converter on the specified context.
    /// </summary>
    protected abstract ValueTask<IOptional> ExecuteConverterAsync<T>(TConverter converter, TConverterContext context);

    /// <summary>
    /// Constructs a command context from the parsed arguments and the current state of the <see cref="ConverterContext"/>.
    /// </summary>
    /// <param name="converterContext">The context used for the argument converters.</param>
    /// <param name="parsedArguments">The arguments successfully parsed by the argument converters.</param>
    /// <returns>The constructed command context.</returns>
    public abstract TCommandContext CreateCommandContext(TConverterContext converterContext, IReadOnlyDictionary<CommandParameter, object?> parsedArguments);
}

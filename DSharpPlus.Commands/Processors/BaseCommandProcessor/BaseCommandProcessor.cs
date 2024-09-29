using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Converters.Results;
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
public abstract partial class BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>
    : ICommandProcessor
    where TConverter : class, IArgumentConverter
    where TConverterContext : ConverterContext
    where TCommandContext : CommandContext
{
    /// <inheritdoc />
    public Type ContextType => typeof(TCommandContext);

    /// <inheritdoc />
    public abstract IReadOnlyList<Command> Commands { get; }

    /// <inheritdoc cref="ICommandProcessor.Converters" />
    public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } =
        new Dictionary<Type, TConverter>();

    /// <inheritdoc />
    IReadOnlyDictionary<Type, IArgumentConverter> ICommandProcessor.Converters =>
        Unsafe.As<IReadOnlyDictionary<Type, IArgumentConverter>>(this.Converters);

    /// <summary>
    /// A dictionary of argument converter delegates indexed by the output type they convert to.
    /// </summary>
    public IReadOnlyDictionary<Type, ConverterDelegate> ConverterDelegates { get; protected set; } =
        new Dictionary<Type, ConverterDelegate>();

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
    protected ILogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>> logger =
        NullLogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>>.Instance;

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
    public virtual void AddConverter(Type type, TConverter converter) =>
        AddConverter(new(this, type, converter));

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
            if (
                type.IsAbstract
                || type.IsInterface
                || type.IsGenericTypeDefinition
                || !type.IsAssignableTo(typeof(TConverter))
            )
            {
                continue;
            }

            // Check if the type implements IArgumentConverter<T>
            Type? genericArgumentConverter = type.GetInterfaces()
                .FirstOrDefault(type =>
                    type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<>)
                );
            if (genericArgumentConverter is null)
            {
                BaseCommandLogging.invalidArgumentConverterImplementation(
                    this.logger,
                    type.FullName ?? type.Name,
                    typeof(IArgumentConverter<>).FullName ?? typeof(IArgumentConverter<>).Name,
                    null
                );

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
        if (
            this.converterFactories.TryGetValue(
                factory.ParameterType,
                out ConverterDelegateFactory? existingFactory
            )
        )
        {
            // If it's a different factory trying to be added, log it.
            // If it's the same factory that's being readded (likely
            // from a gateway disconnect), ignore it.
            if (existingFactory != factory)
            {
                BaseCommandLogging.duplicateArgumentConvertersRegistered(
                    this.logger,
                    factory.ToString()!,
                    factory.ParameterType.FullName ?? factory.ParameterType.Name,
                    existingFactory.ToString()!,
                    null
                );
            }

            return;
        }

        this.converterFactories.Add(factory.ParameterType, factory);
    }

    /// <summary>
    /// Finds all parameters that are enums and creates a generic enum converter for them.
    /// </summary>
    protected virtual void AddEnumConverters()
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("The processor has not been configured yet.");
        }

        // Double check to make sure the enum converter can be casted to TConverter.
        if (!typeof(TConverter).IsAssignableFrom(typeof(EnumConverter<>)))
        {
            BaseCommandLogging.invalidEnumConverterImplementation(
                this.logger,
                typeof(EnumConverter<>).FullName ?? typeof(EnumConverter<>).Name,
                typeof(TConverter).FullName ?? typeof(TConverter).Name,
                GetType().FullName ?? GetType().Name,
                nameof(AddEnumConverters),
                null
            );
        }

        // For every enum type found, add the enum converter to it directly.
        Dictionary<Type, TConverter> enumConverterCache = [];
        foreach (
            Command command in this.extension.Commands.Values.SelectMany(command =>
                command.Flatten()
            )
        )
        {
            foreach (CommandParameter parameter in command.Parameters)
            {
                Type baseType = IArgumentConverter.GetConverterFriendlyBaseType(parameter.Type);
                if (!baseType.IsEnum)
                {
                    continue;
                }

                // Try to reuse any existing enum converters for this enum type.
                if (!enumConverterCache.TryGetValue(baseType, out TConverter? enumConverter))
                {
                    Type genericConverter = typeof(EnumConverter<>).MakeGenericType(baseType);
                    ConstructorInfo? constructor =
                        genericConverter.GetConstructor([])
                        ?? throw new UnreachableException(
                            $"The generic enum converter {genericConverter.FullName!} does not have a parameterless constructor."
                        );
                    enumConverter = (TConverter)constructor.Invoke([]);
                    enumConverterCache.Add(baseType, enumConverter);
                }

                AddConverter(baseType, enumConverter);
            }
        }
    }

    /// <inheritdoc />
    [MemberNotNull(nameof(extension))]
    public virtual ValueTask ConfigureAsync(CommandsExtension extension)
    {
        this.extension = extension;
        this.logger =
            extension.ServiceProvider.GetService<
                ILogger<BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>>
            >()
            ?? NullLogger<
                BaseCommandProcessor<TConverter, TConverterContext, TCommandContext>
            >.Instance;

        // Register all converters from the processor's assembly
        AddConverters(GetType().Assembly);

        // This goes through all command parameters and creates the generic version of the enum converters.
        AddEnumConverters();

        // Populate the default converters
        Dictionary<Type, TConverter> converters = [];
        Dictionary<Type, ConverterDelegate> converterDelegates = [];
        foreach (KeyValuePair<Type, ConverterDelegateFactory> factory in this.converterFactories)
        {
            converters.Add(factory.Key, factory.Value.GetConverter(extension.ServiceProvider));
            converterDelegates.Add(
                factory.Key,
                factory.Value.GetConverterDelegate(extension.ServiceProvider)
            );
        }

        this.Converters = converters.ToFrozenDictionary();
        this.ConverterDelegates = converterDelegates.ToFrozenDictionary();
        return default;
    }

    /// <summary>
    /// Parses the arguments provided to the command and returns a prepared command context.
    /// </summary>
    /// <param name="converterContext">The context used for the argument converters.</param>
    /// <returns>The prepared CommandContext.</returns>
    public virtual async ValueTask<
        IReadOnlyDictionary<CommandParameter, object?>
    > ParseParametersAsync(TConverterContext converterContext)
    {
        // If there's no parameters, begone.
        if (converterContext.Command.Parameters.Count == 0)
        {
            return FrozenDictionary<CommandParameter, object?>.Empty;
        }

        // Populate the parsed arguments with <see cref="ArgumentNotParsedResult"/>
        // to indicate that the arguments haven't been parsed yet.
        // If this method ever exits early without finishing parsing, the
        // callee will know where the argument parsing stopped.
        Dictionary<CommandParameter, object?> parsedArguments =
            new(converterContext.Command.Parameters.Count);
        foreach (CommandParameter parameter in converterContext.Command.Parameters)
        {
            parsedArguments.Add(parameter, new ArgumentNotParsedResult());
        }

        while (converterContext.NextParameter())
        {
            object? parsedArgument = await ParseParameterAsync(converterContext);
            parsedArguments[converterContext.Parameter] = parsedArgument;
            if (parsedArgument is ArgumentFailedConversionResult)
            {
                // Stop parsing if the argument failed to convert.
                // The other parameters will be set to <see cref="ArgumentNotParsedResult"/>.
                // ...XML docs don't work in comments. Pretend they do <3
                break;
            }
        }

        return parsedArguments;
    }

    /// <summary>
    /// Parses a single parameter from the command context. This method will handle <see cref="MultiArgumentAttribute"/> annotated parameters.
    /// </summary>
    /// <param name="converterContext">The converter context containing all the relevant data for the argument parsing.</param>
    public virtual async ValueTask<object?> ParseParameterAsync(TConverterContext converterContext)
    {
        if (this.extension is null)
        {
            throw new InvalidOperationException("The processor has not been configured yet.");
        }
        else if (!converterContext.NextArgument())
        {
            // Try to fill with it's default value
            return converterContext.Parameter.DefaultValue.HasValue
                ? converterContext.Parameter.DefaultValue.Value
                : new ArgumentNotParsedResult();
        }

        try
        {
            ConverterDelegate converterDelegate = this.ConverterDelegates[
                IArgumentConverter.GetConverterFriendlyBaseType(converterContext.Parameter.Type)
            ];
            IOptional optional = await converterDelegate(converterContext);
            if (optional.HasValue)
            {
                // Thanks Roslyn for not yelling at me to make a ternary operator.
                return optional.RawValue;
            }

            // If there's invalid input, the argument converter should throw.
            // Returning an Optional<T> with no value means that the argument converter
            // expected this case and intentionally failed.
            // We return a special value here to indicate that the argument failed conversion,
            // which allows the callee to choose how to handle the failure
            // (e.g. return an error message or selecting the default value).
            return new ArgumentFailedConversionResult();
        }
        catch (Exception error)
        {
            // If an exception occurs during argument parsing, parsing is immediately stopped.
            // We'll set the current parameter to an error state and return the parsed arguments.
            // The callee will choose how to handle the error.
            return new ArgumentFailedConversionResult
            {
                Error = error,
                Value = converterContext.Argument,
            };
        }
    }

    /// <summary>
    /// Executes an argument converter on the specified context.
    /// </summary>
    protected virtual async ValueTask<IOptional> ExecuteConverterAsync<T>(
        TConverter converter,
        TConverterContext context
    )
    {
        if (converter is not IArgumentConverter<T> typedConverter)
        {
            throw new InvalidOperationException(
                $"The converter {converter.GetType().FullName} does not implement IArgumentConverter<{typeof(T).FullName}>."
            );
        }

        // If the parameter is a multi-argument parameter or params, we'll
        // parse all the arguments until we reach the maximum argument count.
        if (
            context.Parameter.Attributes.FirstOrDefault(attribute =>
                attribute is MultiArgumentAttribute
            )
            is not MultiArgumentAttribute multiArgumentAttribute
        )
        {
            return await typedConverter.ConvertAsync(context);
        }

        // TODO: Find a way to use an array pool for this.
        // There's two ways that come to mind, both of which involve
        // subscribing to CommandsExtension.CommandExecuted/CommandErrored:
        // 1. Use ArrayPool<object?>.Shared and have the processor recollect the used
        //    arrays after the commands finishes executing or erroring. As there are multiple
        //    processors that could be running at the same time, which means our event handler
        //    would need to track which command was invoked by this processor... Such as a
        //    AbstractContext.Id property that's tracked by the processor.
        // 2. Add a `ArrayPoolBufferWriter<object?>` as a transient service to the DI container.
        //    Have the (single) event handler iterate over multi-argument parameters return the
        //    arrays to the pool.
        // Either way, I'm not doing those now because I believe said
        // optimization would be better done by the hands of Akiraveliara. <3
        // This 100% isn't just me trying to get out of work - Lunar
        List<T> multiArgumentValues = [];

        // int.MaxValue is used to indicate that there's no maximum argument count.
        // If there's a maximum argument count, we'll ensure that the list has enough
        // capacity to store all the arguments.
        // `params` parameters are treated as multi-argument parameters with no maximum argument count.
        // Due to `params` being semi-used, let's not allocate 2+ gigabytes of memory for a single parameter.
        if (multiArgumentAttribute.MaximumArgumentCount != int.MaxValue)
        {
            multiArgumentValues.EnsureCapacity(multiArgumentAttribute.MaximumArgumentCount);
        }

        // This is a do-while loop because we called NextArgument() at the top of the method.
        do
        {
            Optional<T> parsedArgument = await typedConverter.ConvertAsync(context);
            if (!parsedArgument.HasValue)
            {
                // If the argument converter failed, we might
                // have reached the end of the arguments.
                // Return what we have now, the next time this
                // method is invoked, we'll be able to determine if
                // the argument failed to convert or if there are
                // no more arguments for this parameter.
                return Optional.FromValue<ArgumentFailedConversionResult>(
                    new() { Value = context.Argument }
                );
            }

            multiArgumentValues.Add(parsedArgument.Value);
        } while (context.NextArgument());

        if (multiArgumentValues.Count < multiArgumentAttribute.MinimumArgumentCount)
        {
            // If the minimum argument count isn't met, we'll return an error.
            // The callee will choose how to handle the error.
            return Optional.FromValue<ArgumentFailedConversionResult>(
                new()
                {
                    Error = new ArgumentException(
                        $"The parameter {context.Parameter.Name} requires at least {multiArgumentAttribute.MinimumArgumentCount:N0} arguments, but only {multiArgumentValues.Count:N0} were provided."
                    ),
                    Value = multiArgumentValues.ToArray(),
                }
            );
        }

        // Oh my heart </3
        return context.Parameter.Type.IsArray
            ? Optional.FromValue<T[]>(multiArgumentValues.ToArray())
            : Optional.FromValue<List<T>>(multiArgumentValues);
    }

    /// <summary>
    /// Constructs a command context from the parsed arguments and the current state of the <see cref="ConverterContext"/>.
    /// </summary>
    /// <param name="converterContext">The context used for the argument converters.</param>
    /// <param name="parsedArguments">The arguments successfully parsed by the argument converters.</param>
    /// <returns>The constructed command context.</returns>
    public abstract TCommandContext CreateCommandContext(
        TConverterContext converterContext,
        IReadOnlyDictionary<CommandParameter, object?> parsedArguments
    );
}

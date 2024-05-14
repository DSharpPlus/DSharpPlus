using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.Commands.Processors;

public abstract class BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext> : ICommandProcessor<TEventArgs>
    where TEventArgs : DiscordEventArgs
    where TConverter : IArgumentConverter
    where TConverterContext : ConverterContext
    where TCommandContext : CommandContext
{
    /// <summary>
    /// Serves as a sentinel type for attempted conversions.
    /// </summary>
    protected class ConverterSentinel;

    protected class LazyConverter
    {
        public required Type ParameterType { get; init; }

        public ConverterDelegate<TEventArgs>? ConverterDelegate { get; set; }
        public TConverter? ConverterInstance { get; set; }
        public Type? ConverterType { get; set; }

        public ConverterDelegate<TEventArgs> GetConverterDelegate(BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext> processor, IServiceProvider serviceProvider)
        {
            if (this.ConverterDelegate is not null)
            {
                return this.ConverterDelegate;
            }

            this.ConverterInstance ??= GetConverter(serviceProvider);

            MethodInfo executeConvertAsyncMethod = processor.GetType().GetMethod(nameof(ExecuteConverterAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConverterAsync)} does not exist");
            MethodInfo genericExecuteConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(this.ParameterType) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConverterAsync)} does not exist");
            return this.ConverterDelegate = (ConverterContext converterContext, TEventArgs eventArgs) => (ValueTask<IOptional>)genericExecuteConvertAsyncMethod.Invoke(processor, [this.ConverterInstance, converterContext, eventArgs])!;
        }

        public TConverter GetConverter(IServiceProvider serviceProvider)
        {
            if (this.ConverterInstance is not null)
            {
                return this.ConverterInstance;
            }
            else if (this.ConverterType is null)
            {
                if (this.ConverterDelegate is null)
                {
                    throw new InvalidOperationException("No delegate, converter object, or converter type was provided.");
                }

                this.ConverterType = this.ConverterDelegate.Method.DeclaringType ?? throw new InvalidOperationException("No converter type was provided and the delegate's declaring type is null.");
            }

            if (!this.ConverterType.IsAssignableTo(typeof(TConverter)))
            {
                throw new InvalidOperationException($"Type {this.ConverterType.FullName} does not implement {typeof(TConverter).FullName}");
            }

            // Check if the type implements IArgumentConverter<TConverterContext, TEventArgs, T>
            Type genericArgumentConverter = this.ConverterType
                .GetInterfaces()
                .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,,>))
                ?? throw new InvalidOperationException($"Type {this.ConverterType.FullName} does not implement {typeof(IArgumentConverter<,,>).FullName}");

            return (TConverter)ActivatorUtilities.CreateInstance(serviceProvider, this.ConverterType);
        }

        [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary rabbit hole.")]
        public override string? ToString()
        {
            if (this.ConverterDelegate is not null)
            {
                return this.ConverterDelegate.ToString();
            }
            else if (this.ConverterInstance is not null)
            {
                return this.ConverterInstance.ToString();
            }
            else if (this.ConverterType is not null)
            {
                return this.ConverterType.ToString();
            }
            else
            {
                return "<Empty Lazy Converter>";
            }
        }

        public override bool Equals(object? obj) => obj is LazyConverter lazyConverter && Equals(lazyConverter);
        public bool Equals(LazyConverter obj)
        {
            if (this.ParameterType != obj.ParameterType)
            {
                return false;
            }
            else if (this.ConverterDelegate is not null && obj.ConverterDelegate is not null)
            {
                return this.ConverterDelegate.Equals(obj.ConverterDelegate);
            }
            else if (this.ConverterInstance is not null && obj.ConverterInstance is not null)
            {
                return this.ConverterInstance.Equals(obj.ConverterInstance);
            }
            else if (this.ConverterType is not null && obj.ConverterType is not null)
            {
                return this.ConverterType.Equals(obj.ConverterType);
            }

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(this.ParameterType, this.ConverterDelegate, this.ConverterInstance, this.ConverterType);
    }

    public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } = new Dictionary<Type, TConverter>();
    public IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ConverterDelegates { get; protected set; } = new Dictionary<Type, ConverterDelegate<TEventArgs>>();
    // Redirect the interface to use the converter delegates property instead of the converters property
    IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ICommandProcessor<TEventArgs>.Converters => this.ConverterDelegates;

    protected readonly Dictionary<Type, LazyConverter> lazyConverters = [];
    protected CommandsExtension? extension;
    protected ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>> logger = NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

    private static readonly Action<ILogger, string, Exception?> failedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");

    public virtual void AddConverter<T>(TConverter converter) => AddConverter(typeof(T), converter);
    public virtual void AddConverter(Type type, TConverter converter) => AddConverter(new() { ParameterType = type, ConverterInstance = converter });
    public virtual void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
    public virtual void AddConverters(IEnumerable<Type> types)
    {
        foreach (Type type in types)
        {
            // Ignore types that don't have a concrete implementation (abstract classes or interfaces)
            // Additionally ignore types that have open generics (IArgumentConverter<TEventArgs, T>) instead of closed generics (IArgumentConverter<string>)
            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition || !type.IsAssignableTo(typeof(TConverter)))
            {
                continue;
            }

            // Check if the type implements IArgumentConverter<TEventArgs, T>
            Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,,>));
            if (genericArgumentConverter is null)
            {
                BaseCommandLogging.InvalidArgumentConverterImplementation(this.logger, type.FullName ?? type.Name, typeof(IArgumentConverter<,,>).FullName ?? typeof(IArgumentConverter<,,>).Name, null);
                continue;
            }

            // GenericTypeArguments[2] here is the T in IArgumentConverter<TConverterContext, TEventArgs, T>
            AddConverter(new() { ParameterType = genericArgumentConverter.GenericTypeArguments[2], ConverterType = type });
        }
    }

    protected void AddConverter(LazyConverter lazyConverter)
    {
        if (!this.lazyConverters.TryAdd(lazyConverter.ParameterType, lazyConverter))
        {
            LazyConverter existingLazyConverter = this.lazyConverters[lazyConverter.ParameterType];
            if (!lazyConverter.Equals(existingLazyConverter))
            {
                BaseCommandLogging.DuplicateArgumentConvertersRegistered(this.logger, lazyConverter.ToString()!, existingLazyConverter.ParameterType.FullName ?? existingLazyConverter.ParameterType.Name, existingLazyConverter.ToString()!, null);
            }
        }
    }

    [MemberNotNull(nameof(extension))]
    public virtual ValueTask ConfigureAsync(CommandsExtension extension)
    {
        this.extension = extension;
        this.logger = extension.ServiceProvider.GetService<ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>>() ?? NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

        Type thisType = GetType();
        MethodInfo executeConvertAsyncMethod = thisType.GetMethod(nameof(ExecuteConverterAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConverterAsync)} does not exist");
        AddConverters(thisType.Assembly);

        Dictionary<Type, TConverter> converters = [];
        Dictionary<Type, ConverterDelegate<TEventArgs>> converterDelegates = [];
        foreach (LazyConverter lazyConverter in this.lazyConverters.Values)
        {
            converterDelegates.Add(lazyConverter.ParameterType, lazyConverter.GetConverterDelegate(this, extension.ServiceProvider));
            converters.Add(lazyConverter.ParameterType, lazyConverter.GetConverter(extension.ServiceProvider));
        }

        this.Converters = converters.ToFrozenDictionary();
        this.ConverterDelegates = converterDelegates.ToFrozenDictionary();
        return default;
    }

    public virtual async ValueTask<TCommandContext?> ParseArgumentsAsync(TConverterContext converterContext, TEventArgs eventArgs)
    {
        if (this.extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = new(converterContext.Command.Parameters.Count);
        
        foreach (CommandParameter parameter in converterContext.Command.Parameters)
        {
            parsedArguments.Add(parameter, new ConverterSentinel());
        }

        try
        {
            while (converterContext.NextParameter())
            {
                if (converterContext.Argument is null)
                {
                    continue;
                }

                IOptional optional = await this.ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);
                
                if (!optional.HasValue)
                {
                    await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                    {
                        Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                        Exception = new ArgumentParseException(converterContext.Parameter, null, $"Argument Converter for type {converterContext.Parameter.Type.FullName} was unable to parse the argument."),
                        CommandObject = null
                    });

                    return null;
                }

                parsedArguments[converterContext.Parameter] = optional.RawValue;
            }

            if (parsedArguments.Any(x => x.Value is ConverterSentinel))
            {
                // Try to fill with default values
                foreach (CommandParameter parameter in converterContext.Command.Parameters)
                {
                    if (parsedArguments[parameter] is not ConverterSentinel)
                    {
                        continue;
                    }

                    if (!parameter.DefaultValue.HasValue)
                    {
                        await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                        {
                            Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                            Exception = new ArgumentParseException(converterContext.Parameter, null, "No value was provided for this parameter."),
                            CommandObject = null
                        });

                        return null;
                    }

                    parsedArguments[parameter] = parameter.DefaultValue.Value;
                }
            }
        }
        catch (Exception error)
        {
            await this.extension.commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
            {
                Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return CreateCommandContext(converterContext, eventArgs, parsedArguments);
    }

    public abstract TCommandContext CreateCommandContext(TConverterContext converterContext, TEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments);

    protected virtual Type GetConverterFriendlyBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        Type effectiveType = Nullable.GetUnderlyingType(type) ?? type;

        if (effectiveType.IsEnum)
        {
            return typeof(Enum);
        }
        else if (effectiveType.IsArray)
        {
            return effectiveType.GetElementType()!;
        }

        return effectiveType;
    }

    /// <summary>
    /// Executes an argument converter on the specified context.
    /// </summary>
    protected abstract ValueTask<IOptional> ExecuteConverterAsync<T>
    (
        TConverter converter,
        TConverterContext context,
        TEventArgs eventArgs
    );
}

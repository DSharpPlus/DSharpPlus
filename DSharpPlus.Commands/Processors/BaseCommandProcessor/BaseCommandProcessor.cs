namespace DSharpPlus.Commands.Processors;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Commands.Commands;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public abstract class BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext> : ICommandProcessor<TEventArgs>
    where TEventArgs : DiscordEventArgs
    where TConverter : IArgumentConverter
    where TConverterContext : ConverterContext
    where TCommandContext : CommandContext
{
    protected class LazyConverter
    {
        private delegate Task<IOptional> PrivateConverterDelegate(TConverter converter, TConverterContext converterContext, TEventArgs eventArgs);

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

            this.ConverterInstance ??= this.GetConverter(serviceProvider);

            MethodInfo executeConvertAsyncMethod = processor.GetType().GetMethod(nameof(ExecuteConvertAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
            MethodInfo genericExecuteConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(this.ParameterType) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
            return this.ConverterDelegate = (ConverterContext converterContext, TEventArgs eventArgs) => (Task<IOptional>)genericExecuteConvertAsyncMethod.Invoke(processor, [this.ConverterInstance, converterContext, eventArgs])!;
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
                throw new InvalidOperationException($"Type {this.ConverterType.FullName ?? this.ConverterType.Name} does not implement {typeof(TConverter).FullName ?? typeof(TConverter).Name}");
            }

            // Check if the type implements IArgumentConverter<TEventArgs, T>
            Type genericArgumentConverter = this.ConverterType
                .GetInterfaces()
                .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,>))
                ?? throw new InvalidOperationException($"Type {this.ConverterType.FullName ?? this.ConverterType.Name} does not implement {typeof(IArgumentConverter<,>).FullName ?? typeof(IArgumentConverter<,>).Name}");

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

        public override bool Equals(object? obj) => obj is LazyConverter lazyConverter && this.Equals(lazyConverter);
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

    protected readonly Dictionary<Type, LazyConverter> _lazyConverters = [];
    protected CommandsExtension? _extension;
    protected ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>> _logger = NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

    private static readonly Action<ILogger, string, Exception?> FailedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");

    public virtual void AddConverter<T>(TConverter converter) => this.AddConverter(typeof(T), converter);
    public virtual void AddConverter(Type type, TConverter converter) => this.AddConverter(new() { ParameterType = type, ConverterInstance = converter });
    public virtual void AddConverters(Assembly assembly) => this.AddConverters(assembly.GetTypes());
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
            Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,>));
            if (genericArgumentConverter is null)
            {
                BaseCommandLogging.InvalidArgumentConverterImplementation(this._logger, type.FullName ?? type.Name, typeof(IArgumentConverter<,>).FullName ?? typeof(IArgumentConverter<,>).Name, null);
                continue;
            }

            // GenericTypeArguments[1] here is the T in IArgumentConverter<TEventArgs, T>
            this.AddConverter(new() { ParameterType = genericArgumentConverter.GenericTypeArguments[1], ConverterType = type });
        }
    }

    protected void AddConverter(LazyConverter lazyConverter)
    {
        if (!this._lazyConverters.TryAdd(lazyConverter.ParameterType, lazyConverter))
        {
            LazyConverter existingLazyConverter = this._lazyConverters[lazyConverter.ParameterType];
            if (!lazyConverter.Equals(existingLazyConverter))
            {
                BaseCommandLogging.DuplicateArgumentConvertersRegistered(this._logger, lazyConverter.ToString()!, existingLazyConverter.ParameterType.FullName ?? existingLazyConverter.ParameterType.Name, existingLazyConverter.ToString()!, null);
            }
        }
    }

    [MemberNotNull(nameof(_extension))]
    public virtual Task ConfigureAsync(CommandsExtension extension)
    {
        this._extension = extension;
        this._logger = extension.ServiceProvider.GetService<ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>>() ?? NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

        Type thisType = this.GetType();
        MethodInfo executeConvertAsyncMethod = thisType.GetMethod(nameof(ExecuteConvertAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
        this.AddConverters(thisType.Assembly);

        Dictionary<Type, TConverter> converters = [];
        Dictionary<Type, ConverterDelegate<TEventArgs>> converterDelegates = [];
        foreach (LazyConverter lazyConverter in this._lazyConverters.Values)
        {
            converterDelegates.Add(lazyConverter.ParameterType, lazyConverter.GetConverterDelegate(this, extension.ServiceProvider));
            converters.Add(lazyConverter.ParameterType, lazyConverter.GetConverter(extension.ServiceProvider));
        }

        this.Converters = converters.ToFrozenDictionary();
        this.ConverterDelegates = converterDelegates.ToFrozenDictionary();
        return Task.CompletedTask;
    }

    public virtual async Task<TCommandContext?> ParseArgumentsAsync(TConverterContext converterContext, TEventArgs eventArgs)
    {
        if (this._extension is null)
        {
            return null;
        }

        Dictionary<CommandParameter, object?> parsedArguments = [];
        try
        {
            while (converterContext.NextParameter())
            {
                IOptional optional = await this.ConverterDelegates[this.GetConverterFriendlyBaseType(converterContext.Parameter.Type)](converterContext, eventArgs);
                if (!optional.HasValue)
                {
                    break;
                }

                parsedArguments.Add(converterContext.Parameter, optional.RawValue);
            }
        }
        catch (Exception error)
        {
            await this._extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
            {
                Context = this.CreateCommandContext(converterContext, eventArgs, parsedArguments),
                Exception = new ArgumentParseException(converterContext.Parameter, error),
                CommandObject = null
            });

            return null;
        }

        return this.CreateCommandContext(converterContext, eventArgs, parsedArguments);
    }

    public abstract TCommandContext CreateCommandContext(TConverterContext converterContext, TEventArgs eventArgs, Dictionary<CommandParameter, object?> parsedArguments);

    protected virtual Type GetConverterFriendlyBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (type.IsEnum)
        {
            return typeof(Enum);
        }
        else if (type.IsArray)
        {
            return type.GetElementType()!;
        }

        return Nullable.GetUnderlyingType(type) ?? type;
    }

    protected virtual async Task<IOptional> ExecuteConvertAsync<T>(TConverter converter, TConverterContext converterContext, TEventArgs eventArgs) => await ((IArgumentConverter<TEventArgs, T>)converter).ConvertAsync(converterContext, eventArgs);
}

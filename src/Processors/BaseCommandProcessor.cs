using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.CommandAll.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSharpPlus.CommandAll.Processors
{
    public abstract class BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext> : ICommandProcessor<TEventArgs>
        where TEventArgs : DiscordEventArgs
        where TConverter : IArgumentConverter
        where TConverterContext : ConverterContext
        where TCommandContext : CommandContext
    {
        protected class LazyConverter
        {
            private delegate Task<IOptional> PrivateConverterDelegate(TConverter converter, TConverterContext converterContext, TEventArgs eventArgs);

            public required Type ArgumentType { get; init; }

            public ConverterDelegate<TEventArgs>? ConverterDelegate { get; set; }
            public TConverter? ConverterInstance { get; set; }
            public Type? ConverterType { get; set; }

            public ConverterDelegate<TEventArgs> GetConverterDelegate(BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext> processor, IServiceProvider serviceProvider)
            {
                if (ConverterDelegate is not null)
                {
                    return ConverterDelegate;
                }

                ConverterInstance ??= GetConverter(serviceProvider);

                MethodInfo executeConvertAsyncMethod = processor.GetType().GetMethod(nameof(ExecuteConvertAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                MethodInfo genericExecuteConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(ArgumentType) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                return ConverterDelegate = (ConverterContext converterContext, TEventArgs eventArgs) => (Task<IOptional>)genericExecuteConvertAsyncMethod.Invoke(processor, [ConverterInstance, converterContext, eventArgs])!;
            }

            public TConverter GetConverter(IServiceProvider serviceProvider)
            {
                if (ConverterInstance is not null)
                {
                    return ConverterInstance;
                }
                else if (ConverterType is null)
                {
                    if (ConverterDelegate is null)
                    {
                        throw new InvalidOperationException("No delegate, converter object, or converter type was provided.");
                    }

                    ConverterType = ConverterDelegate.Method.DeclaringType ?? throw new InvalidOperationException("No converter type was provided and the delegate's declaring type is null.");
                }

                if (!ConverterType.IsAssignableTo(typeof(TConverter)))
                {
                    throw new InvalidOperationException($"Type {ConverterType.FullName ?? ConverterType.Name} does not implement {typeof(TConverter).FullName ?? typeof(TConverter).Name}");
                }

                // Check if the type implements IArgumentConverter<TEventArgs, T>
                Type genericArgumentConverter = ConverterType
                    .GetInterfaces()
                    .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,>))
                    ?? throw new InvalidOperationException($"Type {ConverterType.FullName ?? ConverterType.Name} does not implement {typeof(IArgumentConverter<,>).FullName ?? typeof(IArgumentConverter<,>).Name}");

                // GenericTypeArguments[0] here is the T in IArgumentConverter<TEventArgs, T>
                return (TConverter)ActivatorUtilities.CreateInstance(serviceProvider, ConverterType);
            }

            [SuppressMessage("Roslyn", "IDE0046", Justification = "Ternary rabbit hole.")]
            public override string? ToString()
            {
                if (ConverterDelegate is not null)
                {
                    return ConverterDelegate.ToString();
                }
                else if (ConverterInstance is not null)
                {
                    return ConverterInstance.ToString();
                }
                else if (ConverterType is not null)
                {
                    return ConverterType.ToString() ?? ConverterType.Name;
                }
                else
                {
                    return "<Empty Lazy Converter>";
                }
            }

            public override bool Equals(object? obj) => obj is LazyConverter lazyConverter && Equals(lazyConverter);
            public bool Equals(LazyConverter obj)
            {
                if (ArgumentType != obj.ArgumentType)
                {
                    return false;
                }
                else if (ConverterDelegate is not null && obj.ConverterDelegate is not null)
                {
                    return ConverterDelegate.Equals(obj.ConverterDelegate);
                }
                else if (ConverterInstance is not null && obj.ConverterInstance is not null)
                {
                    return ConverterInstance.Equals(obj.ConverterInstance);
                }
                else if (ConverterType is not null && obj.ConverterType is not null)
                {
                    return ConverterType.Equals(obj.ConverterType);
                }

                return false;
            }

            public override int GetHashCode() => HashCode.Combine(ArgumentType, ConverterDelegate, ConverterInstance, ConverterType);
        }

        public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } = new Dictionary<Type, TConverter>();
        public IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ConverterDelegates { get; protected set; } = new Dictionary<Type, ConverterDelegate<TEventArgs>>();
        // Redirect the interface to use the converter delegates property instead of the converters property
        IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ICommandProcessor<TEventArgs>.Converters => ConverterDelegates;

        protected readonly Dictionary<Type, LazyConverter> _lazyConverters = [];
        protected CommandAllExtension? _extension;
        protected ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>> _logger = NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

        private static readonly Action<ILogger, string, Exception?> FailedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");

        public virtual void AddConverter<T>(TConverter converter) => AddConverter(typeof(T), converter);
        public virtual void AddConverter(Type type, TConverter converter) => AddConverter(new() { ArgumentType = type, ConverterInstance = converter });
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
                Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IArgumentConverter<,>));
                if (genericArgumentConverter is null)
                {
                    _logger.LogWarning("Argument Converter {FullName} does not implement {InterfaceFullName}", type.FullName ?? type.Name, typeof(IArgumentConverter<,>).FullName ?? typeof(IArgumentConverter<,>).Name);
                    continue;
                }

                // GenericTypeArguments[1] here is the T in IArgumentConverter<TEventArgs, T>
                AddConverter(new() { ArgumentType = genericArgumentConverter.GenericTypeArguments[1], ConverterType = type });
            }
        }

        protected void AddConverter(LazyConverter lazyConverter)
        {
            if (!_lazyConverters.TryAdd(lazyConverter.ArgumentType, lazyConverter))
            {
                LazyConverter existingLazyConverter = _lazyConverters[lazyConverter.ArgumentType];
                if (!lazyConverter.Equals(existingLazyConverter))
                {
                    _logger.LogError("Failed to add converter {ConverterFullName} because a converter for type {ArgumentType} already exists: {ExistingConverter}", lazyConverter.ToString(), existingLazyConverter.ArgumentType.FullName ?? existingLazyConverter.ArgumentType.Name, existingLazyConverter.ToString());
                }
            }
        }

        [MemberNotNull(nameof(_extension))]
        public virtual Task ConfigureAsync(CommandAllExtension extension)
        {
            _extension = extension;
            _logger = extension.ServiceProvider.GetService<ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>>() ?? NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;

            Type thisType = GetType();
            MethodInfo executeConvertAsyncMethod = thisType.GetMethod(nameof(ExecuteConvertAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
            AddConverters(thisType.Assembly);

            Dictionary<Type, TConverter> converters = [];
            Dictionary<Type, ConverterDelegate<TEventArgs>> converterDelegates = [];
            foreach (LazyConverter lazyConverter in _lazyConverters.Values)
            {
                converterDelegates.Add(lazyConverter.ArgumentType, lazyConverter.GetConverterDelegate(this, extension.ServiceProvider));
                converters.Add(lazyConverter.ArgumentType, lazyConverter.GetConverter(extension.ServiceProvider));
            }

            Converters = converters.ToFrozenDictionary();
            ConverterDelegates = converterDelegates.ToFrozenDictionary();
            return Task.CompletedTask;
        }

        public virtual async Task<TCommandContext?> ParseArgumentsAsync(TConverterContext converterContext, TEventArgs eventArgs)
        {
            if (_extension is null)
            {
                return null;
            }

            Dictionary<CommandArgument, object?> parsedArguments = [];
            try
            {
                while (converterContext.NextArgument())
                {
                    IOptional optional = await ConverterDelegates[GetConverterFriendlyBaseType(converterContext.Argument.Type)](converterContext, eventArgs);
                    if (!optional.HasValue)
                    {
                        break;
                    }

                    parsedArguments.Add(converterContext.Argument, optional.RawValue);
                }
            }
            catch (Exception error)
            {
                await _extension._commandErrored.InvokeAsync(converterContext.Extension, new CommandErroredEventArgs()
                {
                    Context = CreateCommandContext(converterContext, eventArgs, parsedArguments),
                    Exception = new ParseArgumentException(converterContext.Argument, error),
                    CommandObject = null
                });

                return null;
            }

            return CreateCommandContext(converterContext, eventArgs, parsedArguments);
        }

        public abstract TCommandContext CreateCommandContext(TConverterContext converterContext, TEventArgs eventArgs, Dictionary<CommandArgument, object?> parsedArguments);

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
}

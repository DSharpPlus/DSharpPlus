using System;
using System.Collections.Frozen;
using System.Collections.Generic;
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
    public abstract class BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>
        where TEventArgs : DiscordEventArgs
        where TConverter : IArgumentConverter
        where TConverterContext : ConverterContext, new()
        where TCommandContext : CommandContext, new()
    {
        protected record LazyConverter
        {
            public required Type ArgumentType { get; init; }

            public ConverterDelegate<TEventArgs>? Delegate { get; set; }
            public TConverter? Converter { get; set; }
            public Type? ConverterType { get; set; }

            public ConverterDelegate<TEventArgs> GetConverterDelegate(IServiceProvider serviceProvider)
            {
                if (Delegate is not null)
                {
                    return Delegate;
                }

                Converter ??= GetConverter(serviceProvider);
                MethodInfo executeConvertAsyncMethod = GetType().GetMethod(nameof(ExecuteConvertAsync), BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                MethodInfo genericExecuteConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(ArgumentType) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                return Delegate = executeConvertAsyncMethod.CreateDelegate<ConverterDelegate<TEventArgs>>(Converter);
            }

            public TConverter GetConverter(IServiceProvider serviceProvider)
            {
                if (Converter is not null)
                {
                    return Converter;
                }
                else if (ConverterType is null)
                {
                    if (Delegate is null)
                    {
                        throw new InvalidOperationException("No delegate, converter object, or converter type was provided.");
                    }

                    ConverterType = Delegate.Method.DeclaringType ?? throw new InvalidOperationException("No converter type was provided and the delegate's declaring type is null.");
                }

                // Check if the type implements IArgumentConverter<TEventArgs, T>
                Type genericArgumentConverter = ConverterType
                    .GetInterfaces()
                    .FirstOrDefault(type => type.IsAssignableTo(typeof(IArgumentConverter<,>)) && type.IsAssignableTo(typeof(TConverter)))
                    ?? throw new InvalidOperationException($"Type {ConverterType.FullName ?? ConverterType.Name} does not implement {typeof(IArgumentConverter<,>).FullName ?? typeof(IArgumentConverter<,>).Name}");

                // GenericTypeArguments[0] here is the T in IArgumentConverter<TEventArgs, T>
                return (TConverter)ActivatorUtilities.CreateInstance(serviceProvider, ConverterType);
            }
        }

        public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } = new Dictionary<Type, TConverter>();
        public IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ConverterDelegates { get; protected set; } = new Dictionary<Type, ConverterDelegate<TEventArgs>>();

        protected readonly List<LazyConverter> _lazyConverters = [];
        protected CommandAllExtension? _extension;
        protected ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>? _logger;

        private static readonly Action<ILogger, string, Exception?> FailedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");

        public virtual void AddConverter<T>(TConverter converter) => AddConverter(typeof(T), converter);
        public virtual void AddConverter(Type type, TConverter converter) => _lazyConverters.Add(new LazyConverter() { ArgumentType = type, Converter = converter });
        public virtual void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
        public virtual void AddConverters(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                // Ignore types that don't have a concrete implementation (abstract classes or interfaces)
                // Additionally ignore types that have open generics (IArgumentConverter<TEventArgs, T>) instead of closed generics (IArgumentConverter<string>)
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                // Check if the type implements IArgumentConverter<TEventArgs, T>
                Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsAssignableTo(typeof(IArgumentConverter<,>)) && type.IsAssignableTo(typeof(TConverter)));
                if (genericArgumentConverter is null)
                {
                    continue;
                }

                // GenericTypeArguments[1] here is the T in IArgumentConverter<TEventArgs, T>
                _lazyConverters.Add(new LazyConverter() { ArgumentType = genericArgumentConverter.GenericTypeArguments[1], ConverterType = type });
            }
        }

        public virtual Task ConfigureAsync(CommandAllExtension extension)
        {
            _extension = extension;
            _logger = extension.ServiceProvider.GetService<ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>>() ?? NullLogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>.Instance;
            AddConverters(GetType().Assembly);

            Dictionary<Type, TConverter> converters = [];
            Dictionary<Type, ConverterDelegate<TEventArgs>> converterDelegates = [];
            MethodInfo executeConvertAsyncMethod = GetType().GetMethod(nameof(ExecuteConvertAsync), BindingFlags.NonPublic) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
            foreach (LazyConverter lazyConverter in _lazyConverters)
            {
                converterDelegates.Add(lazyConverter.ArgumentType, lazyConverter.GetConverterDelegate(extension.ServiceProvider));
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

        public abstract TCommandContext CreateCommandContext(TConverterContext converterContext, TEventArgs eventArgs, Dictionary<CommandArgument, object?> parsedArguments);
        protected virtual async Task<IOptional> ExecuteConvertAsync<T>(TConverter converter, TConverterContext converterContext, TEventArgs eventArgs) => await ((IArgumentConverter<TEventArgs, T>)converter).ConvertAsync(converterContext, eventArgs);
    }
}

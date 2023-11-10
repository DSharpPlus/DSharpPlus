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
        public IReadOnlyDictionary<Type, TConverter> Converters { get; protected set; } = new Dictionary<Type, TConverter>();
        public IReadOnlyDictionary<Type, ConverterDelegate<TEventArgs>> ConverterDelegates { get; protected set; } = new Dictionary<Type, ConverterDelegate<TEventArgs>>();

        protected readonly Dictionary<Type, TConverter> _converters = [];
        protected CommandAllExtension? _extension;
        protected ILogger<BaseCommandProcessor<TEventArgs, TConverter, TConverterContext, TCommandContext>>? _logger;

        private static readonly Action<ILogger, string, Exception?> FailedConverterCreation = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1), "Failed to create instance of converter '{FullName}' due to a lack of empty public constructors, lack of a service provider, or lack of services within the service provider.");

        public virtual void AddConverter<T>(TConverter converter) => AddConverter(typeof(T), converter);
        public virtual void AddConverter(Type type, TConverter converter) => _converters[type] = converter;
        public virtual void AddConverters(Assembly assembly) => AddConverters(assembly.GetTypes());
        public virtual void AddConverters(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                // Ignore types that don't have a concrete implementation (abstract classes or interfaces)
                // Additionally ignore types that have open generics (IArgumentConverter<T>) instead of closed generics (IArgumentConverter<string>)
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                // Check if the type implements IArgumentConverter<T>
                Type? genericArgumentConverter = type.GetInterfaces().FirstOrDefault(type => type.IsAssignableTo(typeof(IArgumentConverter<,>)) && type.IsAssignableTo(typeof(TConverter)));
                if (genericArgumentConverter is null)
                {
                    continue;
                }

                AddConverter(genericArgumentConverter.GenericTypeArguments[0], (TConverter)Activator.CreateInstance(type)!);

                try
                {
                    object converter;

                    // Check to see if we have a service provider available
                    if (_extension is not null)
                    {
                        // If we do, try to create the converter using the service provider.
                        converter = ActivatorUtilities.CreateInstance(_extension.ServiceProvider, type);
                    }
                    else
                    {
                        // If we don't, try using a parameterless constructor.
                        converter = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Failed to create instance of {type.FullName ?? type.Name}");
                    }

                    // GenericTypeArguments[0] here is the T in IArgumentConverter<T>
                    AddConverter(genericArgumentConverter.GenericTypeArguments[0], (TConverter)converter);
                }
                catch (Exception error)
                {
                    if (_logger is not null)
                    {
                        // Log the error if possible
                        FailedConverterCreation(_logger, type.FullName ?? type.Name, error);
                    }
                }
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
            foreach ((Type type, TConverter converter) in _converters)
            {
                MethodInfo genericExecuteConvertAsyncMethod = executeConvertAsyncMethod.MakeGenericMethod(type) ?? throw new InvalidOperationException($"Method {nameof(ExecuteConvertAsync)} does not exist");
                converterDelegates.Add(type, executeConvertAsyncMethod.CreateDelegate<ConverterDelegate<TEventArgs>>(converter));
                converters.Add(type, converter);
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

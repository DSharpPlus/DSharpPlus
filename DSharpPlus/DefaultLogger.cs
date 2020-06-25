using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    public class DefaultLogger : ILogger<BaseDiscordClient>
    {
        private static readonly object _lock = new object();

        private LogLevel MinimumLevel { get; }
        private string TimestampFormat { get; }

        internal DefaultLogger(BaseDiscordClient client)
            : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
        { }

        internal DefaultLogger(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
        {
            this.MinimumLevel = minLevel;
            this.TimestampFormat = timestampFormat;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
                return;

            lock (_lock)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;

                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;

                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;

                    case LogLevel.Critical:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }

                var message = formatter(state, exception);
                var level = logLevel.ToString().Substring(0, 4);

                Console.Write($"[{DateTimeOffset.Now.ToString(this.TimestampFormat)}] [{eventId.Id,-4}/{eventId.Name,-12}] [{level}]");
                Console.ResetColor();
                Console.WriteLine($" {message}");
                if (exception != null)
                    Console.WriteLine(exception);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
            => logLevel >= this.MinimumLevel;

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

    internal class CompositeDefaultLogger : ILogger<BaseDiscordClient>
    {
        private IEnumerable<ILogger<BaseDiscordClient>> Loggers { get; }

        public CompositeDefaultLogger(IEnumerable<ILoggerProvider> providers)
        {
            this.Loggers = providers.Select(x => x.CreateLogger(typeof(BaseDiscordClient).FullName))
                .OfType<ILogger<BaseDiscordClient>>()
                .ToList();
        }

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            foreach (var logger in this.Loggers)
                logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }

    internal class DefaultLoggerProvider : ILoggerProvider
    {
        private LogLevel MinimumLevel { get; }
        private string TimestampFormat { get; }

        private bool _isDisposed = false;

        internal DefaultLoggerProvider(BaseDiscordClient client)
            : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
        { }

        internal DefaultLoggerProvider(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
        {
            this.MinimumLevel = minLevel;
            this.TimestampFormat = timestampFormat;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (this._isDisposed)
                throw new InvalidOperationException("This logger provider is already disposed.");

            if (categoryName != typeof(BaseDiscordClient).FullName)
                throw new ArgumentException($"This provider can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}.", nameof(categoryName));

            return new DefaultLogger(this.MinimumLevel, this.TimestampFormat);
        }

        public void Dispose()
        {
            this._isDisposed = true;
        }
    }

    internal class DefaultLoggerFactory : ILoggerFactory
    {
        private List<ILoggerProvider> Providers { get; } = new List<ILoggerProvider>();
        private bool _isDisposed = false;

        public void AddProvider(ILoggerProvider provider)
        {
            this.Providers.Add(provider);
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (this._isDisposed)
                throw new InvalidOperationException("This logger factory is already disposed.");

            if (categoryName != typeof(BaseDiscordClient).FullName)
                throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}.", nameof(categoryName));

            return new CompositeDefaultLogger(this.Providers);
        }

        public void Dispose()
        {
            if (this._isDisposed)
                return;
            this._isDisposed = true;

            foreach (var provider in this.Providers)
                provider.Dispose();

            this.Providers.Clear();
        }
    }

    internal class ShardedLoggerFactory : ILoggerFactory
    {
        private ILogger<BaseDiscordClient> Logger { get; }

        public ShardedLoggerFactory(ILogger<BaseDiscordClient> instance)
        {
            this.Logger = instance;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            throw new InvalidOperationException("This is a passthrough logger container, it cannot register new providers.");
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (categoryName != typeof(BaseDiscordClient).FullName)
                throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}.", nameof(categoryName));

            return this.Logger;
        }

        public void Dispose()
        { }
    }
}

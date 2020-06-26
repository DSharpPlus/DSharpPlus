using System;
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
}

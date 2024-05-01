namespace DSharpPlus;

using System;
using Microsoft.Extensions.Logging;

public class DefaultLogger : ILogger<BaseDiscordClient>
{
    private static readonly object _lock = new();

    private LogLevel MinimumLevel { get; }
    private string TimestampFormat { get; }

    internal DefaultLogger(BaseDiscordClient client)
        : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
    { }

    internal DefaultLogger(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
    {
        MinimumLevel = minLevel;
        TimestampFormat = timestampFormat;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        lock (_lock)
        {
            string? ename = eventId.Name;
            ename = ename?.Length > 12 ? ename?[..12] : ename;
            Console.Write($"[{DateTimeOffset.Now.ToString(TimestampFormat)}] [{eventId.Id,-4}/{ename,-12}] ");

            switch (logLevel)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;

                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;

                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogLevel.Critical:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
            }
            Console.Write(logLevel switch
            {
                LogLevel.Trace => "[Trace] ",
                LogLevel.Debug => "[Debug] ",
                LogLevel.Information => "[Info ] ",
                LogLevel.Warning => "[Warn ] ",
                LogLevel.Error => "[Error] ",
                LogLevel.Critical => "[Crit ]",
                LogLevel.None => "[None ] ",
                _ => "[?????] "
            });
            Console.ResetColor();

            //The foreground color is off.
            if (logLevel == LogLevel.Critical)
            {
                Console.Write(" ");
            }

            string message = formatter(state, exception);
            Console.WriteLine(message);
            if (exception != null)
            {
                Console.WriteLine(exception);
            }
        }
    }

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= MinimumLevel;

    public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
}

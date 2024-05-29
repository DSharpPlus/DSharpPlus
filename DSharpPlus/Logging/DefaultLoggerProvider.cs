using System;
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace DSharpPlus.Logging;

internal class DefaultLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, DefaultLogger> loggers = new(StringComparer.Ordinal);
    private readonly LogLevel minimum;
    private readonly string timestampFormat;

    public DefaultLoggerProvider(LogLevel minimum = LogLevel.Trace, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
    {
        this.minimum = minimum;
        this.timestampFormat = timestampFormat;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        if (this.loggers.TryGetValue(categoryName, out DefaultLogger? value))
        {
            return value;
        }
        else
        {
            DefaultLogger logger = new(categoryName, this.minimum, this.timestampFormat);

            return this.loggers.AddOrUpdate
            (
                categoryName,
                logger,
                (_, _) => logger
            );
        }
    }

    public void Dispose() { }
}

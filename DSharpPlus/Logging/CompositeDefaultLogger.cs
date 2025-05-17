using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace DSharpPlus;

internal class CompositeDefaultLogger : ILogger
{
    private IEnumerable<ILogger> Loggers { get; }

    public CompositeDefaultLogger(IEnumerable<ILoggerProvider> providers)
    {
        this.Loggers = providers.Select(x => x.CreateLogger(typeof(BaseDiscordClient).FullName!))
            .ToList();
    }

    public bool IsEnabled(LogLevel logLevel)
        => true;

    public void Log<TState>
    (
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception, string> formatter
    )
    {
        foreach (ILogger logger in this.Loggers)
        {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public IDisposable? BeginScope<TState>(TState state) 
        where TState : notnull 
        => throw new NotImplementedException();
}

using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

using System.Collections.Concurrent;

internal class ShardedLoggerFactory : ILoggerFactory
{
    private ConcurrentDictionary<string, ILogger> Loggers { get; } = new();
    private ILoggerFactory Factory { get; }

    public ShardedLoggerFactory(ILoggerFactory factory) => this.Factory = factory;

    public void AddProvider(ILoggerProvider provider) => throw new InvalidOperationException("This is a passthrough logger container, it cannot register new providers.");

    public ILogger CreateLogger(string categoryName) => this.Loggers.GetOrAdd(categoryName, this.Factory.CreateLogger(categoryName));

    public void Dispose()
    { }
}

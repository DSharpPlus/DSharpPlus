namespace DSharpPlus;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

internal class DefaultLoggerFactory : ILoggerFactory
{
    private List<ILoggerProvider> Providers { get; } = new List<ILoggerProvider>();
    private bool _isDisposed = false;

    public void AddProvider(ILoggerProvider provider) => Providers.Add(provider);

    public ILogger CreateLogger(string categoryName) =>
        _isDisposed
            ? throw new InvalidOperationException("This logger factory is already disposed.")
            : new CompositeDefaultLogger(Providers);

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        foreach (ILoggerProvider provider in Providers)
        {
            provider.Dispose();
        }

        Providers.Clear();
    }
}

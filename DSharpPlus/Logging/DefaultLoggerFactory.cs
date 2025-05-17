using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Logging;

internal class DefaultLoggerFactory : ILoggerFactory
{
    private List<ILoggerProvider> Providers { get; } = [];
    private bool isDisposed = false;

    public void AddProvider(ILoggerProvider provider) => this.Providers.Add(provider);

    public ILogger CreateLogger(string categoryName)
    {
        return this.isDisposed
            ? throw new InvalidOperationException("This logger factory is already disposed.")
            : new CompositeDefaultLogger(this.Providers);
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        foreach (ILoggerProvider provider in this.Providers)
        {
            provider.Dispose();
        }

        this.Providers.Clear();
    }
}

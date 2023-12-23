using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

internal class DefaultLoggerFactory : ILoggerFactory
{
    private List<ILoggerProvider> Providers { get; } = new List<ILoggerProvider>();
    private bool _isDisposed = false;

    public void AddProvider(ILoggerProvider provider) => this.Providers.Add(provider);

    public ILogger CreateLogger(string categoryName) =>
        this._isDisposed
            ? throw new InvalidOperationException("This logger factory is already disposed.")
            : new CompositeDefaultLogger(this.Providers);

    public void Dispose()
    {
        if (this._isDisposed)
        {
            return;
        }

        this._isDisposed = true;

        foreach (ILoggerProvider provider in this.Providers)
        {
            provider.Dispose();
        }

        this.Providers.Clear();
    }
}

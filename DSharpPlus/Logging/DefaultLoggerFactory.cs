using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    internal class DefaultLoggerFactory : ILoggerFactory
    {
        private List<ILoggerProvider> Providers { get; } = new List<ILoggerProvider>();
        private bool _isDisposed = false;

        public void AddProvider(ILoggerProvider provider) => this.Providers.Add(provider);

        public ILogger CreateLogger(string categoryName)
        {
            if (this._isDisposed)
                throw new InvalidOperationException("This logger factory is already disposed.");

            return categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName
                ? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName} or {typeof(DiscordWebhookClient).FullName}.", nameof(categoryName))
                : new CompositeDefaultLogger(this.Providers);
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
}

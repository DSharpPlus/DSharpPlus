using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    internal class ShardedLoggerFactory : ILoggerFactory
    {
        private ILogger<BaseDiscordClient> Logger { get; }

        public ShardedLoggerFactory(ILogger<BaseDiscordClient> instance)
        {
            this.Logger = instance;
        }

        public void AddProvider(ILoggerProvider provider) => throw new InvalidOperationException("This is a passthrough logger container, it cannot register new providers.");

        public ILogger CreateLogger(string categoryName)
        {
            return categoryName != typeof(BaseDiscordClient).FullName
                ? throw new ArgumentException($"This factory can only provide instances of loggers for {typeof(BaseDiscordClient).FullName}.", nameof(categoryName))
                : this.Logger;
        }

        public void Dispose()
        { }
    }
}

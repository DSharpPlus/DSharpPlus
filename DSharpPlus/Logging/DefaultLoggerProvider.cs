using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    internal class DefaultLoggerProvider : ILoggerProvider
    {
        private LogLevel MinimumLevel { get; }
        private string TimestampFormat { get; }

        private bool _isDisposed = false;

        internal DefaultLoggerProvider(BaseDiscordClient client)
            : this(client.Configuration.MinimumLogLevel, client.Configuration.LogTimestampFormat)
        { }

        internal DefaultLoggerProvider(DiscordWebhookClient client)
            : this(client._minimumLogLevel, client._logTimestampFormat)
        { }

        internal DefaultLoggerProvider(LogLevel minLevel = LogLevel.Information, string timestampFormat = "yyyy-MM-dd HH:mm:ss zzz")
        {
            this.MinimumLevel = minLevel;
            this.TimestampFormat = timestampFormat;
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (this._isDisposed)
                throw new InvalidOperationException("This logger provider is already disposed.");

            return categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName
                ? throw new ArgumentException($"This provider can only provide instances of loggers for {typeof(BaseDiscordClient).FullName} or {typeof(DiscordWebhookClient).FullName}.", nameof(categoryName))
                : new DefaultLogger(this.MinimumLevel, this.TimestampFormat);
        }

        public void Dispose() => this._isDisposed = true;
    }
}

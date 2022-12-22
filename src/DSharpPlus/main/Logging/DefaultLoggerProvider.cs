// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using Microsoft.Extensions.Logging;

namespace DSharpPlus;

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
        MinimumLevel = minLevel;
        TimestampFormat = timestampFormat;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _isDisposed
            ? throw new InvalidOperationException("This logger provider is already disposed.")
            : (ILogger)(categoryName != typeof(BaseDiscordClient).FullName && categoryName != typeof(DiscordWebhookClient).FullName
            ? throw new ArgumentException($"This provider can only provide instances of loggers for {typeof(BaseDiscordClient).FullName} or {typeof(DiscordWebhookClient).FullName}.", nameof(categoryName))
            : new DefaultLogger(MinimumLevel, TimestampFormat));
    }

    public void Dispose() => _isDisposed = true;
}

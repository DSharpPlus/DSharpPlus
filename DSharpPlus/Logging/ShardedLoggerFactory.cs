// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
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
using System.Linq;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    internal class ShardedLoggerFactory<T> : ILoggerFactory
    {
        private ILogger<T> Logger { get; }

        private readonly string[] _supportedLoggerTypes = new[]
        {
            typeof(BaseDiscordClient).FullName,
            typeof(DiscordWebhookClient).FullName,
            typeof(DiscordClusterClient).FullName
        };

        public ShardedLoggerFactory(ILogger<T> instance)
        {
            this.Logger = instance;
        }

        public void AddProvider(ILoggerProvider provider) => throw new InvalidOperationException("This is a passthrough logger container, it cannot register new providers.");

        public ILogger CreateLogger(string categoryName)
        {
            if (!this._supportedLoggerTypes.Contains(categoryName))
                throw new ArgumentException($"This factory can only provide instances of loggers for {string.Join(" or ", this._supportedLoggerTypes)}.", nameof(categoryName));
            return this.Logger;
        }

        public void Dispose()
        { }
    }
}

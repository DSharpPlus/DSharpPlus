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
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{

    /// <summary>
    /// Represents a semi-manually sharded client. Used for multi-process sharding.
    /// </summary>
    /// <inheritdoc />
    public sealed class DiscordClusterClient : DiscordShardedClient
    {
        public override async Task StartAsync()
        {
            if (this._isStarted)
                throw new InvalidOperationException("Client is already started.");

            this._isStarted = true;

            try
            {
                if (this.Configuration.TokenType != TokenType.Bot)
                    this.Logger.LogWarning(LoggerEvents.Misc, "You are logging in with a token that is not a bot token. This is not officially supported by Discord, and can result in your account being terminated if you aren't careful.");
                this.Logger.LogInformation(LoggerEvents.Startup, "DSharpPlus, version {Version}", this._versionString.Value);

                var shardc = await this.InitializeShardsAsync().ConfigureAwait(false);
                var connectTasks = new List<Task>();
                this.Logger.LogInformation(LoggerEvents.ShardStartup, "Booting {Shards} shards.", shardc);

                for (var i = 0; i < shardc; i++)
                {
                    //This should never happen, but in case it does...
                    if (this.GatewayInfo.SessionBucket.MaxConcurrency < 1)
                        this.GatewayInfo.SessionBucket.MaxConcurrency = 1;

                    if (this.GatewayInfo.SessionBucket.MaxConcurrency == 1)
                    {
                        await this.ConnectShardAsync(i).ConfigureAwait(false);
                    }
                    else
                    {
                        //Concurrent login.
                        connectTasks.Add(this.ConnectShardAsync(i));

                        if (connectTasks.Count == this.GatewayInfo.SessionBucket.MaxConcurrency)
                        {
                            await Task.WhenAll(connectTasks).ConfigureAwait(false);
                            connectTasks.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await this.StopInternal(false).ConfigureAwait(false);

                var message = "Shard initialization failed, check inner exceptions for details: ";

                this.Logger.LogCritical(LoggerEvents.ShardClientError, $"{message}\n{ex}");
                throw new AggregateException(message, ex);
            }

        }
        public override Task StopAsync() =>
            this.StopInternal(false);

        /// <summary>
        /// Constructs and intiializes a new cluster client.
        /// </summary>
        /// <param name="config">The configuration to initialize the client with.</param>
        public DiscordClusterClient(DiscordConfiguration config)
        {
            this.InternalSetup();
            if (config.ShardCount > 1)
                this._manuallySharding = true;

            this.Configuration = config;
            this.ShardClients = new ReadOnlyConcurrentDictionary<int, DiscordClient>(this._shards);

            if (this.Configuration.LoggerFactory == null)
            {
                this.Configuration.LoggerFactory = new DefaultLoggerFactory();
                this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this.Configuration.MinimumLogLevel, this.Configuration.LogTimestampFormat));
            }
            this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();
        }

        private Task StopInternal(bool enableLogger)
        {
            if (!this._isStarted)
                throw new InvalidOperationException("This client has not been started.");

            if (enableLogger)
                this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disposing {Shards} shards.", this._shards.Count);

            this._isStarted = false;
            this._voiceRegionsLazy = null;

            this.GatewayInfo = null;
            this.CurrentUser = null;
            this.CurrentApplication = null;

            for (var i = 0; i < this._shards.Count; i++)
            {
                if (this._shards.TryGetValue(i, out var client))
                {
                    this.UnhookEventHandlers(client);
                    var cid = client.ShardId;
                    client.Dispose();

                    if (enableLogger)
                        this.Logger.LogInformation(LoggerEvents.ShardShutdown, "Disconnected shard {ShardId}.", cid);
                }
            }

            this._shards.Clear();

            return Task.CompletedTask;
        }

        private async Task<int> InitializeShardsAsync()
        {
            if (this._shards.Count != 0)
                return this._shards.Count;

            this.GatewayInfo = await this.GetGatewayInfoAsync().ConfigureAwait(false);
            var gwShardc = this.Configuration.ShardCount == 1 ? this.GatewayInfo.ShardCount : this.Configuration.ShardCount;
            var shardc = this.Configuration.BootShardCount ?? gwShardc;
            var lf = new ShardedLoggerFactory(this.Logger);
            for (var i = 0; i < shardc; i++)
            {
                var cfg = new DiscordConfiguration(this.Configuration)
                {
                    ShardId = (this.Configuration.ShardId * shardc) + i,
                    ShardCount = gwShardc, /* We want to send the total amount of shards regardless of how many shards this client will boot. */
                    LoggerFactory = lf
                };

                var client = new DiscordClient(cfg);
                if (!this._shards.TryAdd(i, client))
                    throw new InvalidOperationException("Could not initialize shards.");
            }

            return shardc;
        }
    }
}

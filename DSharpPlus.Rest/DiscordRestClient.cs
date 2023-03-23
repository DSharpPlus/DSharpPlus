// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
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

using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace DSharpPlus
{
    public sealed partial class DiscordRestClient : BaseDiscordClient
    {
        /// <summary>
        /// Gets the dictionary of guilds cached by this client.
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds => this._guilds;
        internal Dictionary<ulong, DiscordGuild> _guilds = new();
        private bool _disposed;

        public DiscordRestClient(DiscordConfiguration config) : base(config)
        {
            this._disposed = false;
        }

        /// <summary>
        /// Initializes cache
        /// </summary>
        /// <returns></returns>
        public async Task InitializeCacheAsync()
        {
            await this.InitializeAsync();

            var guilds = await this.ApiClient.GetCurrentUserGuildsAsync(100, null, null);
            foreach (var guild in guilds)
            {
                this._guilds[guild.Id] = guild;
            }
        }


        /// <summary>
        /// Disposes of this DiscordRestClient
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            this._guilds.Clear();
            this.ApiClient._rest.Dispose();
        }
    }
}

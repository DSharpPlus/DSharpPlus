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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.OAuth2
{
    public class DiscordOAuth2Client : BaseDiscordClient
    {
        /// <summary>
        /// This cache is permanently empty, though is implemented as to not break internal caching features.
        /// </summary>
        public override IReadOnlyDictionary<ulong, DiscordGuild> Guilds
            => this._guilds_lazy.Value;

        internal Dictionary<ulong, DiscordGuild> _guilds = new Dictionary<ulong, DiscordGuild>();
        private readonly Lazy<IReadOnlyDictionary<ulong, DiscordGuild>> _guilds_lazy;

        private readonly DiscordOAuth2Config _oAuthConfig;
        private string _refreshToken;
        private bool _disposed;

        public DiscordOAuth2Client(DiscordConfiguration cfg, DiscordOAuth2Config oAuthCfg) : base(cfg)
        {
            if(cfg.TokenType != TokenType.Bearer)
            {
                throw new Exception($"{nameof(DiscordOAuth2Client)} only supports Bearer tokens!");
            }

            this._guilds_lazy = new Lazy<IReadOnlyDictionary<ulong, DiscordGuild>>(() => new ReadOnlyDictionary<ulong, DiscordGuild>(this._guilds));

            this._disposed = false;
            this._oAuthConfig = oAuthCfg;
        }

        public async Task<DiscordTokenResponse> RefreshTokenAsync()
        {
            var token = await this.ApiClient.RefreshTokenAsync(_oAuthConfig.ClientId, _oAuthConfig.ClientSecret, _refreshToken);

            this.Configuration.Token = token.AccessToken;
            this._refreshToken = token.RefreshToken;
            this.ApiClient.RefreshLocalToken();

            return token;
        }

        /// <summary>
        /// Revokes refresh token. Calls <see cref="Dispose"/> as a nice-to-have.
        /// </summary>
        /// <returns></returns>
        public async Task RevokeAsync()
        {
            await this.ApiClient.RevokeTokenAsync(_refreshToken);

            this.Dispose();
        }

        public static async Task<(DiscordOAuth2Client client, DiscordTokenResponse tokens)> FromAuthorizationCodeAsync(string code, DiscordOAuth2Config cfg)
        {
            var discordConfig = new DiscordConfiguration()
            {
                TokenType = TokenType.Bearer,
                Token = ""
            };

            var client = new DiscordOAuth2Client(discordConfig, cfg);

            var token = await client.ApiClient.AuthorizeOAuth2Async(cfg.ClientId, cfg.ClientSecret, cfg.RedirectUri, code);

            client.Configuration.Token = token.AccessToken;
            client._refreshToken = token.RefreshToken;
            client.ApiClient.RefreshLocalToken();

            return (client, token);
        }

        public static async Task<(DiscordOAuth2Client client, DiscordTokenResponse tokens)> FromClientCredentialsAsync(DiscordOAuth2Config cfg, DiscordScopes scopes)
        {
            var discordConfig = new DiscordConfiguration()
            {
                TokenType = TokenType.Bearer,
                Token = ""
            };

            var client = new DiscordOAuth2Client(discordConfig, cfg);

            var token = await client.ApiClient.GetClientCredentialsTokenAsync(cfg.ClientId, cfg.ClientSecret, scopes);

            client.Configuration.Token = token.AccessToken;
            client._refreshToken = token.RefreshToken;
            client.ApiClient.RefreshLocalToken();

            return (client, token);
        }

        /// <summary>
        /// Constructs a new <see cref="DiscordOAuth2Client"/> using a refresh token.
        /// This method throws when the refresh token is invalid. At this point, you'd want to reauthorize the user.
        /// </summary>
        /// <param name="refresh_token">Refresh token belonging to this user.</param>
        /// <param name="clientId">Your app's OAuth2 Client ID</param>
        /// <param name="clientSecret">Your app's OAuth2 Client Secret</param>
        /// <returns>OAuth2 client for current user.</returns>
        public static async Task<(DiscordOAuth2Client client, DiscordTokenResponse tokens)> FromRefreshTokenAsync(DiscordOAuth2Config cfg)
        {
            var discordConfig = new DiscordConfiguration()
            {
                TokenType = TokenType.Bearer,
                Token = ""
            };

            var client = new DiscordOAuth2Client(discordConfig, cfg);

            client._refreshToken = cfg.RefreshToken;
            var token = await client.RefreshTokenAsync();

            return (client, token);
        }

        public override void Dispose()
        {
            if (this._disposed)
                return;
            this._disposed = true;
            this._guilds = null;
            this.ApiClient._rest.Dispose();
        }

        ~DiscordOAuth2Client()
        {
            this.Dispose();
        }
    }
}

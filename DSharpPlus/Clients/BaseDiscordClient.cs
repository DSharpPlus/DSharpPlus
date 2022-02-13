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

#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using Microsoft.Extensions.Logging;

namespace DSharpPlus
{
    /// <summary>
    /// Represents a common base for various Discord client implementations.
    /// </summary>
    public abstract class BaseDiscordClient : IDisposable
    {
        internal protected DiscordApiClient ApiClient { get; }
        internal protected DiscordConfiguration Configuration { get; }

        /// <summary>
        /// Gets the instance of the logger for this client.
        /// </summary>
        public ILogger<BaseDiscordClient> Logger { get; }

        /// <summary>
        /// Gets the string representing the version of D#+.
        /// </summary>
        public string VersionString { get; }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser { get; internal set; }

        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication { get; internal set; }

        /// <summary>
        /// Gets the cached guilds for this client.
        /// </summary>
        public abstract IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

        /// <summary>
        /// Gets the cached users for this client.
        /// </summary>
        protected internal ConcurrentDictionary<ulong, DiscordUser> UserCache { get; }

        /// <summary>
        /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions
            => this._voice_regions_lazy.Value;

        /// <summary>
        /// Gets the list of available voice regions. This property is meant as a way to modify <see cref="VoiceRegions"/>.
        /// </summary>
        protected internal ConcurrentDictionary<string, DiscordVoiceRegion> InternalVoiceRegions { get; set; }
        internal Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>> _voice_regions_lazy;

        /// <summary>
        /// Initializes this Discord API client.
        /// </summary>
        /// <param name="config">Configuration for this client.</param>
        protected BaseDiscordClient(DiscordConfiguration config)
        {
            this.Configuration = new DiscordConfiguration(config);

            if (this.Configuration.LoggerFactory == null)
            {
                this.Configuration.LoggerFactory = new DefaultLoggerFactory();
                this.Configuration.LoggerFactory.AddProvider(new DefaultLoggerProvider(this));
            }
            this.Logger = this.Configuration.LoggerFactory.CreateLogger<BaseDiscordClient>();

            this.ApiClient = new DiscordApiClient(this);
            this.UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
            this.InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
            this._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));

            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
            {
                this.VersionString = iv.InformationalVersion;
            }
            else
            {
                var v = a.GetName().Version;
                var vs = v.ToString(3);

                if (v.Revision > 0)
                    this.VersionString = $"{vs}, CI build {v.Revision}";
            }
        }

        /// <summary>
        /// Gets the current API application.
        /// </summary>
        /// <returns>Current API application.</returns>
        public async Task<DiscordApplication> GetCurrentApplicationAsync()
        {
            var tapp = await this.ApiClient.GetCurrentApplicationInfoAsync().ConfigureAwait(false);
            var app = new DiscordApplication
            {
                Discord = this,
                Id = tapp.Id,
                Name = tapp.Name,
                Description = tapp.Description,
                Summary = tapp.Summary,
                IconHash = tapp.IconHash,
                CustomInstallUrl = tapp.CustomInstallUrl,
                InstallParams = tapp.InstallParams,
                Tags = (tapp.Tags ?? Enumerable.Empty<string>()).ToArray(),
                TermsOfServiceUrl = tapp.TermsOfServiceUrl,
                PrivacyPolicyUrl = tapp.PrivacyPolicyUrl,
                RpcOrigins = tapp.RpcOrigins != null ? new ReadOnlyCollection<string>(tapp.RpcOrigins) : null,
                Flags = tapp.Flags,
                RequiresCodeGrant = tapp.BotRequiresCodeGrant,
                IsPublic = tapp.IsPublicBot,
                CoverImageHash = null
            };

            // do team and owners
            // tbh fuck doing this properly
            if (tapp.Team == null)
            {
                // singular owner

                app.Owners = new ReadOnlyCollection<DiscordUser>(new[] { new DiscordUser(tapp.Owner) });
                app.Team = null;
            }
            else
            {
                // team owner

                app.Team = new DiscordTeam(tapp.Team);

                var members = tapp.Team.Members
                    .Select(x => new DiscordTeamMember(x) { Team = app.Team, User = new DiscordUser(x.User) })
                    .ToArray();

                var owners = members
                    .Where(x => x.MembershipStatus == DiscordTeamMembershipStatus.Accepted)
                    .Select(x => x.User)
                    .ToArray();

                app.Owners = new ReadOnlyCollection<DiscordUser>(owners);
                app.Team.Owner = owners.FirstOrDefault(x => x.Id == tapp.Team.OwnerId);
                app.Team.Members = new ReadOnlyCollection<DiscordTeamMember>(members);
            }

            return app;
        }

        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
            => this.ApiClient.ListVoiceRegionsAsync();

        /// <summary>
        /// Initializes this client. This method fetches information about current user, application, and voice regions.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeAsync()
        {
            if (this.CurrentUser == null)
            {
                this.CurrentUser = await this.ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
                this.UpdateUserCache(this.CurrentUser);
            }

            if (this.Configuration.TokenType == TokenType.Bot && this.CurrentApplication == null)
                this.CurrentApplication = await this.GetCurrentApplicationAsync().ConfigureAwait(false);

            if (this.Configuration.TokenType != TokenType.Bearer && this.InternalVoiceRegions.Count == 0)
            {
                var vrs = await this.ListVoiceRegionsAsync().ConfigureAwait(false);
                foreach (var xvr in vrs)
                    this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
            }
        }

        /// <summary>
        /// Gets the current gateway info for the provided token.
        /// <para>If no value is provided, the configuration value will be used instead.</para>
        /// </summary>
        /// <returns>A gateway info object.</returns>
        public async Task<GatewayInfo> GetGatewayInfoAsync(string token = null)
        {
            if (this.Configuration.TokenType != TokenType.Bot)
                throw new InvalidOperationException("Only bot tokens can access this info.");

            if (string.IsNullOrEmpty(this.Configuration.Token))
            {
                if (string.IsNullOrEmpty(token))
                    throw new InvalidOperationException("Could not locate a valid token.");

                this.Configuration.Token = token;

                var res = await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
                this.Configuration.Token = null;
                return res;
            }

            return await this.ApiClient.GetGatewayInfoAsync().ConfigureAwait(false);
        }

        internal DiscordUser GetCachedOrEmptyUserInternal(ulong user_id)
        {
            this.TryGetCachedUserInternal(user_id, out var user);
            return user;
        }

        internal bool TryGetCachedUserInternal(ulong user_id, out DiscordUser user)
        {
            if (this.UserCache.TryGetValue(user_id, out user))
                return true;

            user = new DiscordUser { Id = user_id, Discord = this };
            return false;
        }

        internal DiscordUser UpdateUserCache(DiscordUser newUser)
        {
            return this.UserCache.AddOrUpdate(newUser.Id, newUser, (id, old) =>
            {
                old.Username = newUser.Username;
                old.Discriminator = newUser.Discriminator;
                old.AvatarHash = newUser.AvatarHash;
                return old;
            });
        }

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

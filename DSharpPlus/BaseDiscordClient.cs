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

namespace DSharpPlus
{
    public abstract class BaseDiscordClient : IDisposable
    {
        internal protected DiscordApiClient ApiClient { get; }
        internal protected DiscordConfiguration Configuration { get; }

        /// <summary>
        /// Gets the instance of the logger for this client.
        /// </summary>
        public DebugLogger DebugLogger { get; }

        /// <summary>
        /// Gets the string representing the version of D#+.
        /// </summary>
        public string VersionString 
            => this._versionString.Value;

        private readonly Lazy<string> _versionString = new Lazy<string>(() =>
        {
            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                return iv.InformationalVersion;

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
                vs = $"{vs}, CI build {v.Revision}";

            return vs;
        });

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
            this.ApiClient = new DiscordApiClient(this);
            this.DebugLogger = new DebugLogger(this);
            this.UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
            this.InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
            this._voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(this.InternalVoiceRegions));
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
                RpcOrigins = tapp.RpcOrigins != null ? new ReadOnlyCollection<string>(tapp.RpcOrigins) : null,
                Flags = 0,
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
                this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
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

        internal DiscordUser GetCachedOrEmptyUserInternal(ulong user_id)
        {
            TryGetCachedUserInternal(user_id, out var user);
            return user;
        }

        internal bool TryGetCachedUserInternal(ulong user_id, out DiscordUser user)
        {
            if (this.UserCache.TryGetValue(user_id, out user))
                return true;

            user = new DiscordUser { Id = user_id, Discord = this };
            return false;
        }

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

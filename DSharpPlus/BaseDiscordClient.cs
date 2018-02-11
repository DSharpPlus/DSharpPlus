#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            => this._version_string.Value;

        private Lazy<string> _version_string = new Lazy<string>(() =>
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
        public Task<DiscordApplication> GetCurrentApplicationAsync() 
            => this.ApiClient.GetCurrentApplicationInfoAsync();

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
            if (this.CurrentUser == null && this.Configuration.TokenType != TokenType.Bearer)
            {
                this.CurrentUser = await this.ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
                this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
            }

            if (this.Configuration.TokenType != TokenType.User && this.CurrentApplication == null)
                this.CurrentApplication = await this.GetCurrentApplicationAsync().ConfigureAwait(false);

            if (this.InternalVoiceRegions.Count == 0)
            {
                var vrs = await this.ListVoiceRegionsAsync().ConfigureAwait(false);
                foreach (var xvr in vrs)
                    this.InternalVoiceRegions.TryAdd(xvr.Id, xvr);
            }
        }

        internal DiscordUser InternalGetCachedUser(ulong user_id) 
            => this.UserCache.TryGetValue(user_id, out var user) ? user : null;

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

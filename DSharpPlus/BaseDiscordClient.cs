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
    public abstract class BaseDiscordClient : PropertyChangedBase, IDisposable
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
            => _version_string.Value;

        private Lazy<string> _version_string = new Lazy<string>(() =>
        {
            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
            {
                return iv.InformationalVersion;
            }

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
            {
                vs = $"{vs}, CI build {v.Revision}";
            }

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
        public ConcurrentDictionary<ulong, DiscordUser> UserCache { get; }

        /// <summary>
        /// Gets the list of available voice regions. Note that this property will not contain VIP voice regions.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordVoiceRegion> VoiceRegions 
            => _voice_regions_lazy.Value;

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
            Configuration = new DiscordConfiguration(config);
            ApiClient = new DiscordApiClient(this);
            DebugLogger = new DebugLogger(this);
            UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
            InternalVoiceRegions = new ConcurrentDictionary<string, DiscordVoiceRegion>();
            _voice_regions_lazy = new Lazy<IReadOnlyDictionary<string, DiscordVoiceRegion>>(() => new ReadOnlyDictionary<string, DiscordVoiceRegion>(InternalVoiceRegions));
        }

        /// <summary>
        /// Gets the current API application.
        /// </summary>
        /// <returns>Current API application.</returns>
        public Task<DiscordApplication> GetCurrentApplicationAsync() 
            => ApiClient.GetCurrentApplicationInfoAsync();

        /// <summary>
        /// Gets a list of regions
        /// </summary>
        /// <returns></returns>
        public Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync() 
            => ApiClient.ListVoiceRegionsAsync();

        /// <summary>
        /// Initializes this client. This method fetches information about current user, application, and voice regions.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeAsync()
        {
            if (CurrentUser == null)
            {
                CurrentUser = await ApiClient.GetCurrentUserAsync().ConfigureAwait(false);
                UserCache.AddOrUpdate(CurrentUser.Id, CurrentUser, (id, xu) => CurrentUser);
            }

            if (Configuration.TokenType != TokenType.User && CurrentApplication == null)
            {
                CurrentApplication = await GetCurrentApplicationAsync().ConfigureAwait(false);
            }

            if (InternalVoiceRegions.Count == 0)
            {
                var vrs = await ListVoiceRegionsAsync().ConfigureAwait(false);
                foreach (var xvr in vrs)
                {
                    InternalVoiceRegions.TryAdd(xvr.Id, xvr);
                }
            }
        }

        internal DiscordUser InternalGetCachedUser(ulong user_id) 
            => UserCache.TryGetValue(user_id, out var user) ? user : null;

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

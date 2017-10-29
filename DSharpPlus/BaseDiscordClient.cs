#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        public string VersionString => this._version_string.Value;
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
        /// Initializes this Discord API client.
        /// </summary>
        /// <param name="config">Configuration for this client.</param>
        protected BaseDiscordClient(DiscordConfiguration config)
        {
            this.Configuration = config;
            this.ApiClient = new DiscordApiClient(this);
            this.DebugLogger = new DebugLogger(this);
            this.UserCache = new ConcurrentDictionary<ulong, DiscordUser>();
        }

        /// <summary>
        /// Gets the current API application.
        /// </summary>
        /// <returns>Current API application.</returns>
        public Task<DiscordApplication> GetCurrentApplicationAsync() =>
            this.ApiClient.GetCurrentApplicationInfoAsync();

        /// <summary>
        /// Initializes this client. This method fetches information about current user and application.
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeAsync()
        {
            if (this.CurrentUser == null)
            {
                this.CurrentUser = await this.ApiClient.GetCurrentUserAsync();
                this.UserCache.AddOrUpdate(this.CurrentUser.Id, this.CurrentUser, (id, xu) => this.CurrentUser);
            }

            if (this.Configuration.TokenType != TokenType.User && this.CurrentApplication == null)
                this.CurrentApplication = await this.GetCurrentApplicationAsync();
        }

        internal DiscordUser InternalGetCachedUser(ulong user_id) =>
            this.UserCache.TryGetValue(user_id, out var user) ? user : null;

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

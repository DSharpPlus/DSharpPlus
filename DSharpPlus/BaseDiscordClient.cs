using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

        internal DiscordUser _current_user;
        /// <summary>
        /// Gets the current user.
        /// </summary>
        public DiscordUser CurrentUser => this._current_user;

        internal DiscordApplication _current_application;
        /// <summary>
        /// Gets the current application.
        /// </summary>
        public DiscordApplication CurrentApplication => this._current_application;

        /// <summary>
        /// Gets the cached guilds for this client.
        /// </summary>
        public abstract IReadOnlyDictionary<ulong, DiscordGuild> Guilds { get; }

        /// <summary>
        /// Initializes this Discord API client.
        /// </summary>
        /// <param name="config">Configuration for this client.</param>
        protected BaseDiscordClient(DiscordConfiguration config)
        {
            this.Configuration = config;
            this.ApiClient = new DiscordApiClient(this);
            this.DebugLogger = new DebugLogger(this);
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
            if (this._current_user == null)
                this._current_user = await this.ApiClient.GetCurrentUserAsync();

            if (this.Configuration.TokenType != TokenType.User && this._current_application == null)
                this._current_application = await this.GetCurrentApplicationAsync();
        }

        /// <summary>
        /// Disposes this client.
        /// </summary>
        public abstract void Dispose();
    }
}

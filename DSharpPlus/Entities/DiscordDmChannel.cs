using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a direct message channel.
    /// </summary>
    public class DiscordDmChannel : DiscordChannel
    {
        internal DiscordDmChannel()
            : base()
        {
            this._recipients_lazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(this._recipients));
        }

        /// <summary>
        /// Gets the recipients of this direct message.
        /// </summary>
        [JsonProperty("recipient", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Recipients 
            => this._recipients_lazy.Value;

        [JsonIgnore]
        internal List<DiscordUser> _recipients;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordUser>> _recipients_lazy;

        /// <summary>
        /// Gets the hash of this channel's icon.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the URL of this channel's icon.
        /// </summary>
        [JsonIgnore]
        public string IconUrl 
            => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/channel-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png" : null;
        
        /// <summary>
        /// Only use for Group DMs! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public Task AddDmRecipientAsync(ulong user_id, string accesstoken, string nickname) 
            => this.Discord.ApiClient.AddGroupDmRecipientAsync(this.Id, user_id, accesstoken, nickname);

        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        public Task RemoveDmRecipientAsync(ulong user_id, string accesstoken) 
            => this.Discord.ApiClient.RemoveGroupDmRecipientAsync(this.Id, user_id);
    }
}

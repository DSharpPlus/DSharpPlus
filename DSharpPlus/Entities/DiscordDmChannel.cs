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
        /// <param name="user_id">The id of the User to add.</param>
        /// <param name="accesstoken">The OAuth2 access token.</param>
        /// <param name="nickname">The nickname to give to the user.</param>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task AddDmRecipientAsync(ulong user_id, string accesstoken, string nickname) 
            => this.Discord.ApiClient.AddGroupDmRecipientAsync(this.Id, user_id, accesstoken, nickname);

        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        /// <param name="user_id">The id of the User to remove.</param>
        /// <param name="accesstoken">The OAuth2 access token.</param>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the channel does not exists.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter exists.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when something unexpected happens on the Discord side.</exception>
        public Task RemoveDmRecipientAsync(ulong user_id, string accesstoken) 
            => this.Discord.ApiClient.RemoveGroupDmRecipientAsync(this.Id, user_id);
    }
}

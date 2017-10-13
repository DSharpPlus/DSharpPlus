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
        {
            _recipientsLazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(_recipients));
        }

        /// <summary>
        /// Gets the recipients of this direct message.
        /// </summary>
        [JsonProperty("recipient", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Recipients => _recipientsLazy.Value;
        [JsonIgnore]
        // ReSharper disable once InconsistentNaming
        internal List<DiscordUser> _recipients;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<DiscordUser>> _recipientsLazy;

        /// <summary>
        /// Gets the hash of this channel's icon.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the URL of this channel's icon.
        /// </summary>
        [JsonIgnore]
        public string IconUrl => !string.IsNullOrWhiteSpace(IconHash) ? $"https://cdn.discordapp.com/channel-icons/{Id.ToString(CultureInfo.InvariantCulture)}/{IconHash}.png" : null;
        
        /// <summary>
        /// Only use for Group DMs! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public Task AddDmRecipientAsync(ulong userId, string accesstoken, string nickname) => Discord.ApiClient.GroupDmAddRecipientAsync(Id, userId, accesstoken, nickname);

        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        public Task RemoveDmRecipientAsync(ulong userId, string accesstoken) => Discord.ApiClient.GroupDmRemoveRecipientAsync(Id, userId);
    }
}

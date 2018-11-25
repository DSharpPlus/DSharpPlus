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
        private string _iconHash;

        internal DiscordDmChannel() : base()
        {
            _recipients_lazy = new Lazy<IReadOnlyList<DiscordUser>>(() => new ReadOnlyCollection<DiscordUser>(_recipients));
        }

        /// <summary>
        /// Gets the recipients of this direct message.
        /// </summary>
        [JsonProperty("recipient", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<DiscordUser> Recipients
            => _recipients_lazy.Value;

        [JsonIgnore]
        internal List<DiscordUser> _recipients;
        [JsonIgnore]
        private Lazy<IReadOnlyList<DiscordUser>> _recipients_lazy;

        /// <summary>
        /// Gets the hash of this channel's icon.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get => _iconHash; internal set => OnPropertySet(ref _iconHash, value); }

        /// <summary>
        /// Gets the URL of this channel's icon.
        /// </summary>
        [JsonIgnore]
        public string IconUrl
            => !string.IsNullOrWhiteSpace(IconHash) ? $"https://cdn.discordapp.com/channel-icons/{Id.ToString(CultureInfo.InvariantCulture)}/{IconHash}.png" : (_recipients.Count == 1 ? _recipients[0].AvatarUrl : null);

        [JsonIgnore]
        public DiscordUser Recipient => _recipients?.Count == 1 ? _recipients[0] : null;

        /// <summary>
        /// Only use for Group DMs! Whitelised bots only. Requires user's oauth2 access token
        /// </summary>
        public Task AddDmRecipientAsync(ulong user_id, string accesstoken, string nickname)
            => Discord.ApiClient.GroupDmAddRecipientAsync(Id, user_id, accesstoken, nickname);

        /// <summary>
        /// Only use for Group DMs!
        /// </summary>
        public Task RemoveDmRecipientAsync(ulong user_id, string accesstoken)
            => Discord.ApiClient.GroupDmRemoveRecipientAsync(Id, user_id);
    }
}

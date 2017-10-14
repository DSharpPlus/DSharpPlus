using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    public sealed class DiscordGuildEmoji : DiscordEmoji
    {
        /// <summary>
        /// Gets the user that created this emoji.
        /// </summary>
        [JsonIgnore]
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the guild to which this emoji belongs.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild { get; internal set; }

        internal DiscordGuildEmoji() { }

        /// <summary>
        /// Modifies this emoji.
        /// </summary>
        /// <param name="name">New name for this emoji.</param>
        /// <param name="roles">Roles for which this emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns>The modified emoji.</returns>
        public Task<DiscordGuildEmoji> ModifyAsync(string name, IEnumerable<DiscordRole> roles = null, string reason = null) 
            => this.Guild.ModifyEmojiAsync(this, name, roles, reason);

        /// <summary>
        /// Deletes this emoji.
        /// </summary>
        /// <param name="reason">Reason for audit log.</param>
        /// <returns></returns>
        public Task DeleteAsync(string reason = null) 
            => this.Guild.DeleteEmojiAsync(this, reason);
    }
}

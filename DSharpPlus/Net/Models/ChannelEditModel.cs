using DSharpPlus.Entities;

namespace DSharpPlus.Net.Models
{
    public class ChannelEditModel : BaseEditModel
    {
        /// <summary>
        /// Sets the channel's new name.
        /// </summary>
        public string Name { internal get; set; }

        /// <summary>
        /// Sets the channel's new position.
        /// </summary>
        public int? Position { internal get; set; }

        /// <summary>
        /// Sets the channel's new topic.
        /// </summary>
        public Optional<string> Topic { internal get; set; }

        /// <summary>
        /// Sets whether the channel is to be marked as NSFW.
        /// </summary>
        public bool? Nsfw { internal get; set; }

        /// <summary>
        /// <para>Sets the parent of this channel.</para>
        /// <para>This should be channel with <see cref="DiscordChannel.Type"/> set to <see cref="ChannelType.Category"/>.</para>
        /// </summary>
        public Optional<DiscordChannel> Parent { internal get; set; }

        /// <summary>
        /// Sets the voice channel's new bitrate.
        /// </summary>
        public int? Bitrate { internal get; set; }

        /// <summary>
        /// Sets the voice channel's new bitrate.
        /// </summary>
        public int? Userlimit { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's new slow mode timeout.</para>
        /// <para>Setting this to null or 0 will disable slow mode.</para>
        /// </summary>
        public Optional<int?> PerUserRateLimit { internal get; set; }

        internal ChannelEditModel() { }
    }
}

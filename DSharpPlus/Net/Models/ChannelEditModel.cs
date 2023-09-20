using System.Collections.Generic;
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
        /// <para>Sets the voice channel's new user limit.</para>
        /// <para>Setting this to 0 will disable the user limit.</para>
        /// </summary>
        public int? Userlimit { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's new slow mode timeout.</para>
        /// <para>Setting this to null or 0 will disable slow mode.</para>
        /// </summary>
        public Optional<int?> PerUserRateLimit { internal get; set; }

        /// <summary>
        /// <para>Sets the voice channel's region override.</para>
        /// <para>Setting this to null will set it to automatic.</para>
        /// </summary>
        public Optional<DiscordVoiceRegion> RtcRegion { internal get; set; }

        /// <summary>
        /// <para>Sets the voice channel's video quality.</para>
        /// </summary>
        public VideoQualityMode? QualityMode { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's type.</para>
        /// <para>This can only be used to convert between text and news channels.</para>
        /// </summary>
        public Optional<ChannelType> Type { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's permission overwrites.</para>
        /// </summary>
        public IEnumerable<DiscordOverwriteBuilder> PermissionOverwrites { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's auto-archive duration.</para>
        /// </summary>
        public Optional<AutoArchiveDuration?> DefaultAutoArchiveDuration { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's flags (forum channels and posts only).</para>
        /// </summary>
        public Optional<ChannelFlags> Flags { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's available tags.</para>
        /// </summary>
        public IEnumerable<DiscordForumTagBuilder> AvailableTags { internal get; set; }

        /// <summary>
        /// <para>Sets the channel's default reaction, if any.</para>
        /// </summary>
        public Optional<DefaultReaction?> DefaultReaction { internal get; set; }

        /// <summary>
        /// <para>Sets the default slowmode of newly created threads, but does not retroactively update.</para>
        /// </summary>
        /// <remarks>https://discord.com/developers/docs/resources/channel#modify-channel-json-params-guild-channel</remarks>
        public Optional<int> DefaultThreadRateLimit { internal get; set; }

        /// <summary>
        /// Sets the default sort order of posts in this channel.
        /// </summary>
        public Optional<DefaultSortOrder?> DefaultSortOrder { internal get; set; }

        /// <summary>
        /// Sets the default layout of posts in this channel.
        /// </summary>
        public Optional<DefaultForumLayout> DefaultForumLayout { internal get; set; }

        internal ChannelEditModel() { }
    }
}

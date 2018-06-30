using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Entities
{
    public class DiscordUserSettings : PropertyChangedBase
    {
        private string _theme;

        [JsonProperty("timezone_offset")]
        public long TimezoneOffset { get; set; }

        [JsonProperty("theme")]
        public string Theme
        {
            get => _theme;
            set
            {
                OnPropertySet(ref _theme, value);
            }
        }        

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("show_current_game")]
        public bool ShowCurrentGame { get; set; }

        [JsonProperty("restricted_guilds")]
        public IList<ulong> RestrictedGuilds { get; set; }

        [JsonProperty("render_reactions")]
        public bool RenderReactions { get; set; }

        [JsonProperty("render_embeds")]
        public bool RenderEmbeds { get; set; }

        [JsonProperty("message_display_compact")]
        public bool MessageDisplayCompact { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("inline_embed_media")]
        public bool InlineEmbedMedia { get; set; }

        [JsonProperty("inline_attachment_media")]
        public bool InlineAttachmentMedia { get; set; }

        [JsonProperty("guild_positions")]
        public IList<ulong> GuildPositions { get; set; }

        [JsonProperty("gif_auto_play")]
        public bool GifAutoPlay { get; set; }

        // TODO: ????
        // [JsonProperty("friend_source_flags")]
        // public FriendSourceFlags FriendSourceFlags { get; set; }

        [JsonProperty("explicit_content_filter")]
        public int ExplicitContentFilter { get; set; }

        [JsonProperty("enable_tts_command")]
        public bool EnableTtsCommand { get; set; }

        [JsonProperty("developer_mode")]
        public bool DeveloperMode { get; set; }

        [JsonProperty("detect_platform_accounts")]
        public bool DetectPlatformAccounts { get; set; }

        [JsonProperty("default_guilds_restricted")]
        public bool DefaultGuildsRestricted { get; set; }

        [JsonProperty("convert_emoticons")]
        public bool ConvertEmoticons { get; set; }

        [JsonProperty("animate_emoji")]
        public bool AnimateEmoji { get; set; }

        [JsonProperty("afk_timeout")]
        public int AfkTimeout { get; set; }
    }
}

using Newtonsoft.Json;
using System;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord Sticker.
    /// </summary>
    public class DiscordMessageSticker : SnowflakeObject, IEquatable<DiscordMessageSticker>
    {
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        private string _internalTags;

        /// <summary>
        /// Gets the Pack ID of this sticker.
        /// </summary>
        [JsonProperty("pack_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong PackId { get; internal set; }

        /// <summary>
        /// Gets the Name of the sticker.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the Description of the sticker.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the list of tags for the sticker.
        /// </summary>
        public string[] Tags => _internalTags.Split(',');

        /// <summary>
        /// Gets the asset hash of the sticker.
        /// </summary>
        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public string Asset { get; internal set; }

        /// <summary>
        /// Gets the preview asset hash of the sticker.
        /// </summary>
        [JsonProperty("preview_asset", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewAsset { get; internal set; }

        /// <summary>
        /// Gets the Format type of the sticker.
        /// </summary>
        [JsonProperty("format_type", NullValueHandling = NullValueHandling.Ignore)]
        public StickerFormat FormatType { get; internal set; }

        public bool Equals(DiscordMessageSticker other)
        {
            return this.Id == other.Id;
        }
    }

    public enum StickerFormat
    {
        PNG = 1,
        APNG = 2,
        LOTTIE = 3
    }
}

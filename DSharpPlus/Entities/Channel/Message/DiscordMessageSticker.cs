using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord Sticker.
    /// </summary>
    public class DiscordMessageSticker : SnowflakeObject, IEquatable<DiscordMessageSticker>
    {
        /// <summary>
        /// Gets the Pack ID of this sticker.
        /// </summary>
        [JsonProperty("pack_id")]
        public ulong PackId { get; internal set; }

        /// <summary>
        /// Gets the Name of the sticker.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the Description of the sticker.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the list of tags for the sticker.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Tags
            => this._internalTags != null ? this._internalTags.Split(',') : Array.Empty<string>();

        /// <summary>
        /// Gets the asset hash of the sticker.
        /// </summary>
        [JsonProperty("asset")]
        public string Asset { get; internal set; }

        /// <summary>
        /// Gets the preview asset hash of the sticker.
        /// </summary>
        [JsonProperty("preview_asset", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewAsset { get; internal set; }

        /// <summary>
        /// Gets the Format type of the sticker.
        /// </summary>
        [JsonProperty("format_type")]
        public StickerFormat FormatType { get; internal set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        private string _internalTags { get; set; }

        public bool Equals(DiscordMessageSticker other) => this.Id == other.Id;
    }

    public enum StickerFormat
    {
        PNG = 1,
        APNG = 2,
        LOTTIE = 3
    }
}

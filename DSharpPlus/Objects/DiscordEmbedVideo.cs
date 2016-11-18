using Newtonsoft.Json;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordEmbedVideo
    {
        /// <summary>
        /// Source url of the video
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        /// <summary>
        /// Height of video
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
        /// <summary>
        /// Width of video
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }
    }
}

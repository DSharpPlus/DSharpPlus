using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordAttachment : SnowflakeObject
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        [JsonProperty("filename")]
        public string FileName { get; internal set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        [JsonProperty("size")]
        public int FileSize { get; internal set; }

        /// <summary>
        /// File URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; internal set; }

        /// <summary>
        /// Proxy file URL
        /// </summary>
        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; internal set; }

        /// <summary>
        /// Height (if image)
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; internal set; }

        /// <summary>
        /// Width (if image)
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; internal set; }
    }
}

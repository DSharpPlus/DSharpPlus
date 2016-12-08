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
<<<<<<< HEAD
        [JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("filename")]
>>>>>>> master
        public string FileName { get; internal set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("size")]
>>>>>>> master
        public int FileSize { get; internal set; }

        /// <summary>
        /// File URL
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("url")]
>>>>>>> master
        public string Url { get; internal set; }

        /// <summary>
        /// Proxy file URL
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("proxy_url")]
>>>>>>> master
        public string ProxyUrl { get; internal set; }

        /// <summary>
        /// Height (if image)
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("height")]
>>>>>>> master
        public int Height { get; internal set; }

        /// <summary>
        /// Width (if image)
        /// </summary>
<<<<<<< HEAD
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
=======
        [JsonProperty("width")]
>>>>>>> master
        public int Width { get; internal set; }
    }
}

// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an attachment for a message.
    /// </summary>
    public class DiscordAttachment : SnowflakeObject
    {
        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        [JsonProperty("filename", NullValueHandling = NullValueHandling.Ignore)]
        public string FileName { get; internal set; }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int FileSize { get; internal set; }

        /// <summary>
        /// Gets the media, or MIME, type of the file.
        /// </summary>
        [JsonProperty("content_type", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaType { get; internal set; }

        /// <summary>
        /// Gets the URL of the file.
        /// </summary>
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; internal set; }

        /// <summary>
        /// Gets the proxied URL of the file.
        /// </summary>
        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ProxyUrl { get; internal set; }

        /// <summary>
        /// Gets the height. Applicable only if the attachment is an image.
        /// </summary>
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public int? Height { get; internal set; }

        /// <summary>
        /// Gets the width. Applicable only if the attachment is an image.
        /// </summary>
        [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
        public int? Width { get; internal set; }

        /// <summary>
        /// Gets whether this attachment is ephemeral.
        /// </summary>
        [JsonProperty("ephemeral", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ephemeral { get; internal set; }

        internal DiscordAttachment() { }
    }
}

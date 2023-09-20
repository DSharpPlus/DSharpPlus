using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DSharpPlus.Entities;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a multipart HTTP request.
    /// </summary>
    internal sealed class MultipartWebRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the dictionary of values attached to this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>
        /// Gets the dictionary of files attached to this request.
        /// </summary>
        public IReadOnlyCollection<DiscordMessageFile> Files { get; }

        internal bool _removeFileCount;

        internal MultipartWebRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, string route, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null,
            IReadOnlyCollection<DiscordMessageFile> files = null, double? ratelimit_wait_override = null, bool removeFileCount = false)
            : base(client, bucket, url, method, route, headers, ratelimit_wait_override)
        {
            this.Values = values;
            this.Files = files;
            this._removeFileCount = removeFileCount;
        }
    }
}

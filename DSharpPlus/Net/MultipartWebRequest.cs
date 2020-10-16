using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
        public IReadOnlyDictionary<string, Stream> Files { get; }

        internal MultipartWebRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IReadOnlyDictionary<string, string> headers = null, IReadOnlyDictionary<string, string> values = null, 
            IReadOnlyDictionary<string, Stream> files = null, double? ratelimit_wait_override = null)
            : base(client, bucket, url, method, headers, ratelimit_wait_override)
        {
            this.Values = values;
            this.Files = files;
        }
    }
}
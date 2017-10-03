using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a multipart HTTP request.
    /// </summary>
    public sealed class MultipartWebRequest : BaseRestRequest
    {
        /// <summary>
        /// Gets the dictionary of values attached to this request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Values { get; }

        /// <summary>
        /// Gets the dictionary of files attached to this request.
        /// </summary>
        public IReadOnlyDictionary<string, Stream> Files { get; }

        internal MultipartWebRequest(BaseDiscordClient client, RateLimitBucket bucket, Uri url, RestRequestMethod method, IDictionary<string, string> headers = null, IDictionary<string, string> values = null, 
            IDictionary<string, Stream> files = null, double? ratelimit_wait_override = null)
            : base(client, bucket, url, method, headers, ratelimit_wait_override)
        {
            this.Values = values != null ? new ReadOnlyDictionary<string, string>(values) : null;
            this.Files = files != null ? new ReadOnlyDictionary<string, Stream>(files) : null;
        }
    }
}
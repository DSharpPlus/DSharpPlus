using System.Collections.Generic;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a response sent by the remote HTTP party.
    /// </summary>
    public sealed class RestResponse
    {
        /// <summary>
        /// Gets the response code sent by the remote party.
        /// </summary>
        public int ResponseCode { get; internal set; }

        /// <summary>
        /// Gets the headers sent by the remote party.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; internal set; }

        /// <summary>
        /// Gets the contents of the response sent by the remote party.
        /// </summary>
        public string Response { get; internal set; }

        internal RestResponse() { }
    }
}

using System.Net.Http;

namespace DSharpPlus.Core.Rest
{
    /// <summary>
    /// Represents a contract for all different rest requests, specifying essential information for the
    /// <seealso cref="RestClient"/> and providing a <seealso cref="HttpRequestMessage"/>.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface IRestRequest
    {
        /// <summary>
        /// Indicates the ratelimit endpoint for this request. 
        /// See: <seealso href="https://discord.com/developers/docs/topics/rate-limits"/>.
        /// </summary>
        public string Endpoint { get; }

        /// <summary>
        /// Specifies whether this specific request is subject to global ratelimits.
        /// </summary>
        public bool IsSubjectToGlobalLimit { get; }

        /// <summary>
        /// Constructs a request message from the provided data.
        /// </summary>
        public HttpRequestMessage Build();
    }
}

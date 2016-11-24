using System;
using System.Runtime.Serialization;

namespace DSharpPlus.Objects
{
    public class RateLimitException : Exception
    {
        /// <summary>
        /// The amount of time to retry the request after in ms.
        /// </summary>
        public int RetryAfter { get; private set; }

        public RateLimitException()
        {}

        public RateLimitException(string message) : base(message)
        {}

        public RateLimitException(string message, Exception innerException) : base(message, innerException)
        {}

        public RateLimitException(string message, int retryAfter) : base(message) { RetryAfter = retryAfter; }

        protected RateLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}

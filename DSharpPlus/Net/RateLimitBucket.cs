using System;

namespace DSharpPlus.Net
{
    /// <summary>
    /// Represents a rate limit bucket.
    /// </summary>
    internal class RateLimitBucket : IEquatable<RateLimitBucket>
    {
        /// <summary>
        /// Gets the value of the parameter by which the requests are bucketed.
        /// </summary>
        public ulong Parameter { get; internal set; }

        /// <summary>
        /// Gets the url by which the requests are bucketed.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Gets the type of the parameter by which the requests are bucketed.
        /// </summary>
        public MajorParameterType ParameterType { get; internal set; }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        public RestRequestMethod Method { get; internal set; }

        /// <summary>
        /// Gets the number of uses left before pre-emptive rate limit is triggered.
        /// </summary>
        public int Remaining { get; internal set; }

        /// <summary>
        /// Gets the maximum number of uses within a single bucket.
        /// </summary>
        public int Maximum { get; internal set; }

        /// <summary>
        /// Gets the timestamp at which the rate limit resets.
        /// </summary>
        public DateTimeOffset Reset { get; internal set; }

        /// <summary>
        /// Returns a string representation of this bucket.
        /// </summary>
        /// <returns>String representation of this bucket.</returns>
        public override string ToString()
        {
            return $"Rate limit bucket [{this.Parameter}:{this.ParameterType}] [{Remaining}/{Maximum}] {Reset}";
        }

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="RateLimitBucket"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as RateLimitBucket);
        }

        /// <summary>
        /// Checks whether this <see cref="RateLimitBucket"/> is equal to another <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <param name="e"><see cref="RateLimitBucket"/> to compare to.</param>
        /// <returns>Whether the <see cref="RateLimitBucket"/> is equal to this <see cref="RateLimitBucket"/>.</returns>
        public bool Equals(RateLimitBucket e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.ParameterType == e.ParameterType && ((this.ParameterType == MajorParameterType.Unbucketed && this.Path == e.Path) || (this.ParameterType != MajorParameterType.Unbucketed && this.Parameter == e.Parameter));
        }

        /// <summary>
        /// Gets the hash code for this <see cref="RateLimitBucket"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="RateLimitBucket"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + this.ParameterType.GetHashCode();
            hash = (hash * 7) + this.Method.GetHashCode();
            hash *= 7;

            if (this.ParameterType != MajorParameterType.Unbucketed)
                hash += this.Parameter.GetHashCode();
            else
                hash += this.Path.GetHashCode();

            return hash;
        }
    }

    /// <summary>
    /// Represents the type of major parameter according to which requests are bucketed.
    /// </summary>
    internal enum MajorParameterType
    {
        /// <summary>
        /// Parameter type is unknown or parameter missing.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Requests are bucketed per-channel.
        /// </summary>
        Channel = 1,

        /// <summary>
        /// Requests are bucketed per-guild.
        /// </summary>
        Guild = 2,

        /// <summary>
        /// Requests are bucketed per-path.
        /// </summary>
        Unbucketed = 3
    }
}

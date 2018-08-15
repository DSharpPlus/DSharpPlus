#pragma warning disable 0649

using Newtonsoft.Json;
using System;

namespace DSharpPlus.Lavalink
{
    public struct LavalinkTrack
    {
        /// <summary>
        /// Gets or sets the ID of the track to play.
        /// </summary>
        [JsonIgnore]
        public string TrackString { get; set; }

        /// <summary>
        /// Gets the identifier of the track.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; internal set; }

        /// <summary>
        /// Gets whether the track is seekable.
        /// </summary>
        [JsonProperty("isSeekable")]
        public bool IsSeekable { get; internal set; }

        /// <summary>
        /// Gets the author of the track.
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; internal set; }

        /// <summary>
        /// Gets the track's duration.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Length => !this.IsStream ? TimeSpan.FromMilliseconds(this._length) : TimeSpan.Zero;
        [JsonProperty("length")]
        internal long _length;

        /// <summary>
        /// Gets whether the track is a stream.
        /// </summary>
        [JsonProperty("isStream")]
        public bool IsStream { get; internal set; }

        /// <summary>
        /// Gets the starting position of the track.
        /// </summary>
        [JsonIgnore]
        public TimeSpan Position => TimeSpan.FromMilliseconds(this._position);
        [JsonProperty("position")]
        internal long _position;

        /// <summary>
        /// Gets the title of the track.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; internal set; }

        /// <summary>
        /// Gets the source Uri of this track.
        /// </summary>
        [JsonProperty("uri")]
        public Uri Uri { get; internal set; }
    }
}

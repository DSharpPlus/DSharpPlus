#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSharpPlus.Lavalink;

public class LavalinkTrack
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
    public TimeSpan Length => !IsStream ? TimeSpan.FromMilliseconds(_length) : TimeSpan.Zero;
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
    public TimeSpan Position => TimeSpan.FromMilliseconds(_position);
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

/// <summary>
/// Represents Lavalink track loading results.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum LavalinkLoadResultType
{
    /// <summary>
    /// Specifies that track was loaded successfully.
    /// </summary>
    [EnumMember(Value = "TRACK_LOADED")]
    TrackLoaded,

    /// <summary>
    /// Specifies that playlist was loaded successfully.
    /// </summary>
    [EnumMember(Value = "PLAYLIST_LOADED")]
    PlaylistLoaded,

    /// <summary>
    /// Specifies that the result set contains search results.
    /// </summary>
    [EnumMember(Value = "SEARCH_RESULT")]
    SearchResult,

    /// <summary>
    /// Specifies that the search yielded no results.
    /// </summary>
    [EnumMember(Value = "NO_MATCHES")]
    NoMatches,

    /// <summary>
    /// Specifies that the track failed to load.
    /// </summary>
    [EnumMember(Value = "LOAD_FAILED")]
    LoadFailed
}

/// <summary>
/// Represents information about playlist that was loaded by Lavalink.
/// </summary>
public struct LavalinkPlaylistInfo
{
    /// <summary>
    /// Gets the name of the playlist being loaded.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the index of the track that was selected in this playlist.
    /// </summary>
    [JsonProperty("selectedTrack")]
    public int SelectedTrack { get; internal set; }
}

/// <summary>
/// Represents information about track loading request.
/// </summary>
public class LavalinkLoadResult
{
    /// <summary>
    /// Gets the loading result type for this request.
    /// </summary>
    [JsonProperty("loadType")]
    public LavalinkLoadResultType LoadResultType { get; internal set; }

    /// <summary>
    /// <para>Gets the information about the playlist loaded as a result of this request.</para>
    /// <para>Only applicable if <see cref="LoadResultType"/> is set to <see cref="LavalinkLoadResultType.PlaylistLoaded"/>.</para>
    /// </summary>
    [JsonProperty("playlistInfo")]
    public LavalinkPlaylistInfo PlaylistInfo { get; internal set; }

    /// <summary>
    /// Gets the exception details if a track loading failed.
    /// </summary>
    [JsonProperty("exception", NullValueHandling = NullValueHandling.Ignore)]
    public LavalinkLoadFailedInfo Exception { get; internal set; }

    /// <summary>
    /// Gets the tracks that were loaded as a result of this request.
    /// </summary>
    //[JsonProperty("tracks")]
    [JsonIgnore]
    public IEnumerable<LavalinkTrack> Tracks { get; internal set; }
}

/// <summary>
/// Represents properties sent when a Lavalink track is unable to load.
/// </summary>
public struct LavalinkLoadFailedInfo
{
    /// <summary>
    /// Gets the message of the sent exception.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; internal set; }

    /// <summary>
    /// Gets the severity level of the track loading failure.
    /// </summary>
    [JsonProperty("severity")]
    public LoadFailedSeverity Severity { get; internal set; }
}

/// <summary>
/// Represents severity level of the track loading failure.
/// </summary>
public enum LoadFailedSeverity
{
    /// <summary>
    /// Indicates a known cause for the failure, and not because of Lavaplayer.
    /// </summary>
    [EnumMember(Value = "COMMON")]
    Common,

    /// <summary>
    /// Indicates an unknown cause for the failure, most likely caused by outside sources.
    /// </summary>
    [EnumMember(Value = "SUSPICIOUS")]
    Suspicious,

    /// <summary>
    /// Indicates an issue with Lavaplayer or otherwise no other way to determine the cause.
    /// </summary>
    [EnumMember(Value = "FAULT")]
    Fault
}

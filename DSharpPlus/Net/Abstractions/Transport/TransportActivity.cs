namespace DSharpPlus.Net.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents a game a user is playing.
/// </summary>
internal sealed class TransportActivity
{
    /// <summary>
    /// Gets or sets the name of the game the user is playing.
    /// </summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets or sets the stream URI, if applicable.
    /// </summary>
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string StreamUrl { get; internal set; }

    /// <summary>
    /// Gets or sets the livesteam type.
    /// </summary>
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordActivityType ActivityType { get; internal set; }

    /// <summary>
    /// Gets or sets the details.
    ///
    /// <para>This is a component of the rich presence, and, as such, can only be used by regular users.</para>
    /// </summary>
    [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
    public string Details { get; internal set; }

    /// <summary>
    /// Gets or sets game state.
    ///
    /// <para>This is a component of the rich presence, and, as such, can only be used by regular users.</para>
    /// </summary>
    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string State { get; internal set; }

    /// <summary>
    /// Gets the emoji details for a custom status, if any.
    /// </summary>
    [JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
    public DiscordEmoji Emoji { get; internal set; }

    /// <summary>
    /// Gets ID of the application for which this rich presence is for.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonIgnore]
    public ulong? ApplicationId
    {
        get => ApplicationIdStr != null ? (ulong?)ulong.Parse(ApplicationIdStr, CultureInfo.InvariantCulture) : null;
        internal set => ApplicationIdStr = value?.ToString(CultureInfo.InvariantCulture);
    }

    [JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
    internal string ApplicationIdStr { get; set; }

    /// <summary>
    /// Gets or sets instance status.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Instance { get; internal set; }

    /// <summary>
    /// Gets or sets information about the current game's party.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonProperty("party", NullValueHandling = NullValueHandling.Ignore)]
    public GameParty Party { get; internal set; }

    /// <summary>
    /// Gets or sets information about assets related to this rich presence.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
    public PresenceAssets Assets { get; internal set; }

    /// <summary>
    /// Gets or sets information about current game's timestamps.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
    public GameTimestamps Timestamps { get; internal set; }

    /// <summary>
    /// Gets or sets information about current game's secret values.
    ///
    /// This is a component of the rich presence, and, as such, can only be used by regular users.
    /// </summary>
    [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
    public GameSecrets Secrets { get; internal set; }

    [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<string> Buttons { get; internal set; }

    internal TransportActivity() { }

    internal TransportActivity(DiscordActivity game)
    {
        if (game == null)
        {
            return;
        }

        Name = game.Name;
        State = game.CustomStatus?.Name!;
        ActivityType = game.ActivityType;
        StreamUrl = game.StreamUrl;
    }

    public bool IsRichPresence()
        => Details != null || State != null || ApplicationId != null || Instance != null || Party != null || Assets != null || Secrets != null || Timestamps != null;

    public bool IsCustomStatus()
        => Name == "Custom Status";

    /// <summary>
    /// Represents information about assets attached to a rich presence.
    /// </summary>
    public class PresenceAssets
    {
        /// <summary>
        /// Gets the large image asset ID.
        /// </summary>
        [JsonProperty("large_image")]
        public string LargeImage { get; set; }

        /// <summary>
        /// Gets the large image text.
        /// </summary>
        [JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
        public string LargeImageText { get; internal set; }

        /// <summary>
        /// Gets the small image asset ID.
        /// </summary>
        [JsonProperty("small_image")]
        internal string SmallImage { get; set; }

        /// <summary>
        /// Gets the small image text.
        /// </summary>
        [JsonProperty("small_text", NullValueHandling = NullValueHandling.Ignore)]
        public string SmallImageText { get; internal set; }
    }

    /// <summary>
    /// Represents information about rich presence game party.
    /// </summary>
    public class GameParty
    {
        /// <summary>
        /// Gets the game party ID.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the size of the party.
        /// </summary>
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public GamePartySize Size { get; internal set; }

        /// <summary>
        /// Represents information about party size.
        /// </summary>
        [JsonConverter(typeof(GamePartySizeConverter))]
        public class GamePartySize
        {
            /// <summary>
            /// Gets the current number of players in the party.
            /// </summary>
            public long Current { get; internal set; }

            /// <summary>
            /// Gets the maximum party size.
            /// </summary>
            public long Maximum { get; internal set; }
        }
    }

    /// <summary>
    /// Represents information about the game state's timestamps.
    /// </summary>
    public class GameTimestamps
    {
        /// <summary>
        /// Gets the time the game has started.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? Start
            => _start != null ? (DateTimeOffset?)Utilities.GetDateTimeOffsetFromMilliseconds(_start.Value, false) : null;

        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        internal long? _start;

        /// <summary>
        /// Gets the time the game is going to end.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset? End
            => _end != null ? (DateTimeOffset?)Utilities.GetDateTimeOffsetFromMilliseconds(_end.Value, false) : null;

        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        internal long? _end;
    }

    /// <summary>
    /// Represents information about secret values for the Join, Spectate, and Match actions.
    /// </summary>
    public class GameSecrets
    {
        /// <summary>
        /// Gets the secret value for join action.
        /// </summary>
        [JsonProperty("join", NullValueHandling = NullValueHandling.Ignore)]
        public string Join { get; internal set; }

        /// <summary>
        /// Gets the secret value for match action.
        /// </summary>
        [JsonProperty("match", NullValueHandling = NullValueHandling.Ignore)]
        public string Match { get; internal set; }

        /// <summary>
        /// Gets the secret value for spectate action.
        /// </summary>
        [JsonProperty("spectate", NullValueHandling = NullValueHandling.Ignore)]
        public string Spectate { get; internal set; }
    }
}

internal sealed class GamePartySizeConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        object[]? obj = value is TransportActivity.GameParty.GamePartySize sinfo
            ? new object[] { sinfo.Current, sinfo.Maximum }
            : null;
        serializer.Serialize(writer, obj);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JArray arr = ReadArrayObject(reader, serializer);
        return new TransportActivity.GameParty.GamePartySize
        {
            Current = (long)arr[0],
            Maximum = (long)arr[1],
        };
    }

    private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer) => serializer.Deserialize<JToken>(reader) is not JArray arr || arr.Count != 2
            ? throw new JsonSerializationException("Expected array of length 2")
            : arr;

    public override bool CanConvert(Type objectType) => objectType == typeof(TransportActivity.GameParty.GamePartySize);
}

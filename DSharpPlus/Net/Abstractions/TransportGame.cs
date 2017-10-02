using System;
using System.Globalization;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Net.Abstractions
{
    /// <summary>
    /// Represents a game a user is playing.
    /// </summary>
    public sealed class TransportGame
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
        public string Url { get; internal set; }

        /// <summary>
        /// Gets or sets the livesteam type.
        /// </summary>
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public GameStreamType StreamType { get; internal set; }

        /// <summary>
        /// Gets or sets the details.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        [JsonProperty("details", NullValueHandling = NullValueHandling.Ignore)]
        public string Details { get; internal set; }

        /// <summary>
        /// Gets or sets game state.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; internal set; }

        /// <summary>
        /// Gets ID of the application for which this rich presence is for.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        [JsonIgnore]
        public ulong? ApplicationId
        {
            get { return this.ApplicationIdStr != null ? (ulong?)ulong.Parse(this.ApplicationIdStr, CultureInfo.InvariantCulture) : null; }
            internal set
            {
                this.ApplicationIdStr = value?.ToString(CultureInfo.InvariantCulture);
            }
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
        /// Gets or sets infromation about current game's timestamps.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public GameTimestamps Timestamps { get; internal set; }

        /// <summary>
        /// Gets or sets infromation about current game's secret values.
        /// 
        /// This is a component of the rich presence, and, as such, can only be used by regular users.
        /// </summary>
        [JsonProperty("secrets", NullValueHandling = NullValueHandling.Ignore)]
        public GameSecrets Secrets { get; internal set; }

        internal TransportGame() { }
        internal TransportGame(DiscordGame game)
        {
            if (game == null)
                return;

            this.Name = game.Name;
            this.StreamType = game.StreamType;
            this.Url = game.Url;

            //this.Details = game.Details;
            //this.State = game.State;
            //this.Instance = game.Instance;
            //this.ApplicationId = game.Application?.Id;
            //
            //if (game.PartyId != null || (game.CurrentPartySize != null && game.MaximumPartySize != null))
            //{
            //    this.Party = new GameParty
            //    {
            //        Id = game.PartyId?.ToString(CultureInfo.InvariantCulture)
            //    };
            //
            //    if (game.CurrentPartySize != null && game.MaximumPartySize != null)
            //        this.Party.Size = new GameParty.GamePartySize
            //        {
            //            Current = game.CurrentPartySize.Value,
            //            Maximum = game.MaximumPartySize.Value
            //        };
            //}
            //
            //if (game.LargeImage != null && game.SmallImage != null)
            //    this.Assets = new PresenceAssets
            //    {
            //        LargeImage = game.LargeImage.Id,
            //        SmallImage = game.SmallImage.Id,
            //        LargeImageText = game.LargeImageText,
            //        SmallImageText = game.SmallImageText
            //    };
            //
            //if (game.StartTimestamp != null || game.EndTimestamp != null)
            //    this.Timestamps = new GameTimestamps
            //    {
            //        _start = game.StartTimestamp != null ? (long?)(Utilities.GetUnixTime(game.StartTimestamp.Value) / 1000) : null,
            //        _end = game.EndTimestamp != null ? (long?)(Utilities.GetUnixTime(game.EndTimestamp.Value) / 1000) : null
            //    };
            //
            //if (game.JoinSecret != null || game.MatchSecret != null || game.SpectateSecret != null)
            //    this.Secrets = new GameSecrets
            //    {
            //        Join = game.JoinSecret,
            //        Match = game.MatchSecret,
            //        Spectate = game.SpectateSecret
            //    };
        }

        /// <summary>
        /// Represents information about assets attached to a rich presence.
        /// </summary>
        public class PresenceAssets
        {
            /// <summary>
            /// Gets the large image asset ID.
            /// </summary>
            [JsonIgnore]
            public ulong LargeImage
            {
                get { return ulong.Parse(this.LargeImageStr, CultureInfo.InvariantCulture); }
                internal set
                {
                    this.LargeImageStr = value.ToString(CultureInfo.InvariantCulture);
                }
            }
            
            [JsonProperty("large_image")]
            internal string LargeImageStr { get; set; }

            /// <summary>
            /// Gets the large image text.
            /// </summary>
            [JsonProperty("large_text", NullValueHandling = NullValueHandling.Ignore)]
            public string LargeImageText { get; internal set; }

            /// <summary>
            /// Gets the small image asset ID.
            /// </summary>
            [JsonIgnore]
            public ulong SmallImage
            {
                get { return ulong.Parse(this.SmallImageStr, CultureInfo.InvariantCulture); }
                internal set
                {
                    this.SmallImageStr = value.ToString(CultureInfo.InvariantCulture);
                }
            }

            [JsonProperty("small_image")]
            internal string SmallImageStr { get; set; }

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
                public int Current { get; internal set; }

                /// <summary>
                /// Gets the maximum party size.
                /// </summary>
                public int Maximum { get; internal set; }
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
            public DateTimeOffset? Start =>
                this._start != null ? (DateTimeOffset?)Utilities.GetDateTimeOffset(this._start.Value * 1000) : null;

            [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
            internal long? _start;

            /// <summary>
            /// Gets the time the game is going to end.
            /// </summary>
            [JsonIgnore]
            public DateTimeOffset? End =>
                this._end != null ? (DateTimeOffset?)Utilities.GetDateTimeOffset(this._end.Value * 1000) : null;

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
            var sinfo = value as TransportGame.GameParty.GamePartySize;
            var obj = sinfo != null ? new object[] { sinfo.Current, sinfo.Maximum } : null;
            serializer.Serialize(writer, obj);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var arr = ReadArrayObject(reader, serializer);
            return new TransportGame.GameParty.GamePartySize
            {
                Current = (int)arr[0],
                Maximum = (int)arr[1],
            };
        }

        private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer)
        {
            var arr = serializer.Deserialize<JToken>(reader) as JArray;
            if (arr == null || arr.Count != 2)
                throw new JsonSerializationException("Expected array of length 2");
            return arr;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TransportGame.GameParty.GamePartySize);
        }
    }
}

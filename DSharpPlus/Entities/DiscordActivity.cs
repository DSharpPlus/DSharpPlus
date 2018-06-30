using System;
using System.Globalization;
using DSharpPlus.Net.Abstractions;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents user status.
    /// </summary>
    public enum UserStatus : int
    {
        /// <summary>
        /// User is offline.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// User is online.
        /// </summary>
        Online = 1,

        /// <summary>
        /// User is idle.
        /// </summary>
        Idle = 2,

        /// <summary>
        /// User asked not to be disturbed.
        /// </summary>
        DoNotDisturb = 4,

        /// <summary>
        /// User is invisible. They will appear as Offline to anyone but themselves.
        /// </summary>
        Invisible = 5
    }

    /// <summary>
    /// Represents a game that a user is playing.
    /// </summary>
    public class DiscordActivity
    {
        /// <summary>
        /// Gets or sets the name of user's activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the stream URL, if applicable.
        /// </summary>
        public string StreamUrl { get; set; }

        /// <summary>
        /// Gets or sets the activity type.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets the rich presence details, if present.
        /// </summary>
        public DiscordRichPresence RichPresence { get; internal set; }

        /// <summary>
        /// Creates a new, empty instance of a <see cref="DiscordActivity"/>.
        /// </summary>
        public DiscordActivity()
        {
            ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="name">Name of the activity.</param>
        public DiscordActivity(string name)
        {
            Name = name;
            ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="name">Name of the activity.</param>
        /// <param name="type">Type of the activity.</param>
        public DiscordActivity(string name, ActivityType type)
        {
            Name = name;
            ActivityType = type;
        }

        internal DiscordActivity(TransportActivity rawActivity)
        {
            UpdateWith(rawActivity);
        }

        internal DiscordActivity(DiscordActivity other)
        {
            Name = other.Name;
            ActivityType = other.ActivityType;
            StreamUrl = other.StreamUrl;
            RichPresence = new DiscordRichPresence(other.RichPresence);
        }

        internal void UpdateWith(TransportActivity rawActivity)
        {
            Name = rawActivity?.Name;
            ActivityType = rawActivity != null ? rawActivity.ActivityType : ActivityType.Playing;
            StreamUrl = rawActivity?.StreamUrl;

            if (rawActivity?.IsRichPresence() == true && RichPresence != null)
            {
                RichPresence.UpdateWith(rawActivity);
            }
            else if (rawActivity?.IsRichPresence() == true)
            {
                RichPresence = new DiscordRichPresence(rawActivity);
            }
            else
            {
                RichPresence = null;
            }
        }
    }

    /// <summary>
    /// Represents details for Discord rich presence, attached to a <see cref="DiscordActivity"/>.
    /// </summary>
    public class DiscordRichPresence
    {
        /// <summary>
        /// Gets the details.
        /// </summary>
        public string Details { get; internal set; }

        /// <summary>
        /// Gets the game state.
        /// </summary>
        public string State { get; internal set; }

        /// <summary>
        /// Gets the application for which the rich presence is for.
        /// </summary>
        public DiscordApplication Application { get; internal set; }

        /// <summary>
        /// Gets instance status.
        /// </summary>
        public bool? Instance { get; internal set; }

        /// <summary>
        /// Gets large image for the rich presence.
        /// </summary>
        public DiscordAsset LargeImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for large image.
        /// </summary>
        public string LargeImageText { get; internal set; }

        /// <summary>
        /// Gets small image for the rich presence.
        /// </summary>
        public DiscordAsset SmallImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for small image.
        /// </summary>
        public string SmallImageText { get; internal set; }

        /// <summary>
        /// Gets current party size.
        /// </summary>
        public int? CurrentPartySize { get; internal set; }

        /// <summary>
        /// Gets maximum party size.
        /// </summary>
        public int? MaximumPartySize { get; internal set; }

        /// <summary>
        /// Gets the party ID.
        /// </summary>
        public ulong? PartyId { get; internal set; }

        /// <summary>
        /// Gets the game start timestamp.
        /// </summary>
        public DateTimeOffset? StartTimestamp { get; internal set; }

        /// <summary>
        /// Gets the game end timestamp.
        /// </summary>
        public DateTimeOffset? EndTimestamp { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to join your game.
        /// </summary>
        public string JoinSecret { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to receive notifications whenever your game state changes.
        /// </summary>
        public string MatchSecret { get; internal set; }

        /// <summary>
        /// Gets the secret value enabling users to spectate your game.
        /// </summary>
        public string SpectateSecret { get; internal set; }

        internal DiscordRichPresence() { }

        internal DiscordRichPresence(TransportActivity rawGame)
        {
            UpdateWith(rawGame);
        }

        internal DiscordRichPresence(DiscordRichPresence other)
        {
            Details = other.Details;
            State = other.State;
            Application = other.Application;
            Instance = other.Instance;
            LargeImageText = other.LargeImageText;
            SmallImageText = other.SmallImageText;
            LargeImage = other.LargeImage;
            SmallImage = other.SmallImage;
            CurrentPartySize = other.CurrentPartySize;
            MaximumPartySize = other.MaximumPartySize;
            PartyId = other.PartyId;
            StartTimestamp = other.StartTimestamp;
            EndTimestamp = other.EndTimestamp;
            JoinSecret = other.JoinSecret;
            MatchSecret = other.MatchSecret;
            SpectateSecret = other.SpectateSecret;
        }

        internal void UpdateWith(TransportActivity rawGame)
        {
            Details = rawGame?.Details;
            State = rawGame?.State;
            Application = rawGame?.ApplicationId != null ? new DiscordApplication { Id = rawGame.ApplicationId.Value } : null;
            Instance = rawGame?.Instance;
            LargeImageText = rawGame?.Assets?.LargeImageText;
            SmallImageText = rawGame?.Assets?.SmallImageText;
            //this.LargeImage = rawGame?.Assets?.LargeImage != null ? new DiscordApplicationAsset { Application = this.Application, Id = rawGame.Assets.LargeImage.Value, Type = ApplicationAssetType.LargeImage } : null;
            //this.SmallImage = rawGame?.Assets?.SmallImage != null ? new DiscordApplicationAsset { Application = this.Application, Id = rawGame.Assets.SmallImage.Value, Type = ApplicationAssetType.SmallImage } : null;
            CurrentPartySize = rawGame?.Party?.Size?.Current;
            MaximumPartySize = rawGame?.Party?.Size?.Maximum;
            if (rawGame?.Party != null && ulong.TryParse(rawGame.Party.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out var partyId))
            {
                PartyId = partyId;
            }

            StartTimestamp = rawGame?.Timestamps?.Start;
            EndTimestamp = rawGame?.Timestamps?.End;
            JoinSecret = rawGame?.Secrets?.Join;
            MatchSecret = rawGame?.Secrets?.Match;
            SpectateSecret = rawGame?.Secrets?.Spectate;

            var lid = rawGame?.Assets?.LargeImage;
            if (lid != null)
            {
                if (lid.StartsWith("spotify:"))
                {
                    LargeImage = new DiscordSpotifyAsset { Id = lid };
                }
                else if (ulong.TryParse(lid, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulid))
                {
                    LargeImage = new DiscordApplicationAsset { Id = lid, Application = Application, Type = ApplicationAssetType.LargeImage };
                }
            }

            var sid = rawGame?.Assets?.SmallImage;
            if (sid != null)
            {
                if (sid.StartsWith("spotify:"))
                {
                    SmallImage = new DiscordSpotifyAsset { Id = sid };
                }
                else if (ulong.TryParse(sid, NumberStyles.Number, CultureInfo.InvariantCulture, out var usid))
                {
                    SmallImage = new DiscordApplicationAsset { Id = sid, Application = Application, Type = ApplicationAssetType.LargeImage };
                }
            }
        }
    }

    /// <summary>
    /// Determines the type of a user activity.
    /// </summary>
    public enum ActivityType : int
    {
        /// <summary>
        /// Indicates the user is playing a game.
        /// </summary>
        Playing = 0,

        /// <summary>
        /// Indicates the user is streaming a game.
        /// </summary>
        Streaming = 1,

        /// <summary>
        /// Indicates the user is listening to something.
        /// </summary>
        ListeningTo = 2,

        /// <summary>
        /// Indicates the user is watching something.
        /// </summary>
        Watching = 3
    }
}

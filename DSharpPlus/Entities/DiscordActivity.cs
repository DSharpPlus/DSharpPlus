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
            this.ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="name">Name of the activity.</param>
        public DiscordActivity(string name)
        {
            this.Name = name;
            this.ActivityType = ActivityType.Playing;
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DiscordActivity"/> with specified name.
        /// </summary>
        /// <param name="name">Name of the activity.</param>
        /// <param name="type">Type of the activity.</param>
        public DiscordActivity(string name, ActivityType type)
        {
            this.Name = name;
            this.ActivityType = type;
        }

        internal DiscordActivity(TransportActivity rawActivity)
        {
            this.UpdateWith(rawActivity);
        }

        internal DiscordActivity(DiscordActivity other)
        {
            this.Name = other.Name;
            this.ActivityType = other.ActivityType;
            this.StreamUrl = other.StreamUrl;
            this.RichPresence = new DiscordRichPresence(other.RichPresence);
        }

        internal void UpdateWith(TransportActivity rawActivity)
        {
            this.Name = rawActivity?.Name;
            this.ActivityType = rawActivity != null ? rawActivity.ActivityType : ActivityType.Playing;
            this.StreamUrl = rawActivity?.StreamUrl;

            if (rawActivity?.IsRichPresence() == true && this.RichPresence != null)
                this.RichPresence.UpdateWith(rawActivity);
            else if (rawActivity?.IsRichPresence() == true)
                this.RichPresence = new DiscordRichPresence(rawActivity);
            else
                this.RichPresence = null;
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
        public long? CurrentPartySize { get; internal set; }

        /// <summary>
        /// Gets maximum party size.
        /// </summary>
        public long? MaximumPartySize { get; internal set; }

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
            this.UpdateWith(rawGame);
        }

        internal DiscordRichPresence(DiscordRichPresence other)
        {
            this.Details = other.Details;
            this.State = other.State;
            this.Application = other.Application;
            this.Instance = other.Instance;
            this.LargeImageText = other.LargeImageText;
            this.SmallImageText = other.SmallImageText;
            this.LargeImage = other.LargeImage;
            this.SmallImage = other.SmallImage;
            this.CurrentPartySize = other.CurrentPartySize;
            this.MaximumPartySize = other.MaximumPartySize;
            this.PartyId = other.PartyId;
            this.StartTimestamp = other.StartTimestamp;
            this.EndTimestamp = other.EndTimestamp;
            this.JoinSecret = other.JoinSecret;
            this.MatchSecret = other.MatchSecret;
            this.SpectateSecret = other.SpectateSecret;
        }

        internal void UpdateWith(TransportActivity rawGame)
        {
            this.Details = rawGame?.Details;
            this.State = rawGame?.State;
            this.Application = rawGame?.ApplicationId != null ? new DiscordApplication { Id = rawGame.ApplicationId.Value } : null;
            this.Instance = rawGame?.Instance;
            this.LargeImageText = rawGame?.Assets?.LargeImageText;
            this.SmallImageText = rawGame?.Assets?.SmallImageText;
            //this.LargeImage = rawGame?.Assets?.LargeImage != null ? new DiscordApplicationAsset { Application = this.Application, Id = rawGame.Assets.LargeImage.Value, Type = ApplicationAssetType.LargeImage } : null;
            //this.SmallImage = rawGame?.Assets?.SmallImage != null ? new DiscordApplicationAsset { Application = this.Application, Id = rawGame.Assets.SmallImage.Value, Type = ApplicationAssetType.SmallImage } : null;
            this.CurrentPartySize = rawGame?.Party?.Size?.Current;
            this.MaximumPartySize = rawGame?.Party?.Size?.Maximum;
            if (rawGame?.Party != null && ulong.TryParse(rawGame.Party.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out var partyId))
                this.PartyId = partyId;
            this.StartTimestamp = rawGame?.Timestamps?.Start;
            this.EndTimestamp = rawGame?.Timestamps?.End;
            this.JoinSecret = rawGame?.Secrets?.Join;
            this.MatchSecret = rawGame?.Secrets?.Match;
            this.SpectateSecret = rawGame?.Secrets?.Spectate;

            var lid = rawGame?.Assets?.LargeImage;
            if (lid != null)
            {
                if (lid.StartsWith("spotify:"))
                    this.LargeImage = new DiscordSpotifyAsset { Id = lid };
                else if (ulong.TryParse(lid, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulid))
                    this.LargeImage = new DiscordApplicationAsset { Id = lid, Application = this.Application, Type = ApplicationAssetType.LargeImage };
            }

            var sid = rawGame?.Assets?.SmallImage;
            if (sid != null)
            {
                if (sid.StartsWith("spotify:"))
                    this.SmallImage = new DiscordSpotifyAsset { Id = sid };
                else if (ulong.TryParse(sid, NumberStyles.Number, CultureInfo.InvariantCulture, out var usid))
                    this.SmallImage = new DiscordApplicationAsset { Id = sid, Application = this.Application, Type = ApplicationAssetType.LargeImage };
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

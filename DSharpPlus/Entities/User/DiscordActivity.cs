// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents user status.
    /// </summary>
    [JsonConverter(typeof(UserStatusConverter))]
    public enum UserStatus
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

    internal sealed class UserStatusConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is UserStatus status)
            {
                switch (status) // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
                {
                    case UserStatus.Online:
                        writer.WriteValue("online");
                        return;

                    case UserStatus.Idle:
                        writer.WriteValue("idle");
                        return;

                    case UserStatus.DoNotDisturb:
                        writer.WriteValue("dnd");
                        return;

                    case UserStatus.Invisible:
                        writer.WriteValue("invisible");
                        return;

                    case UserStatus.Offline:
                    default:
                        writer.WriteValue("offline");
                        return;
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is
            // offline or invisible, the corresponding field is not present.
            return (reader.Value?.ToString().ToLowerInvariant()) switch // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
            {
                "online" => UserStatus.Online,
                "idle" => UserStatus.Idle,
                "dnd" => UserStatus.DoNotDisturb,
                "invisible" => UserStatus.Invisible,
                _ => UserStatus.Offline,
            };
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(UserStatus);
    }

    /// <summary>
    /// Represents a game that a user is playing.
    /// </summary>
    public sealed class DiscordActivity
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
        /// Gets the custom status of this activity, if present.
        /// </summary>
        public DiscordCustomStatus CustomStatus { get; internal set; }

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
            if (type == ActivityType.Custom)
            {
                this.Name = "Custom Status";
                this.CustomStatus = new DiscordCustomStatus() { Name = name };
                this.ActivityType = ActivityType.Custom;
            }
            else
            {
                this.Name = name;
                this.ActivityType = type;
            }

            
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
            if(other.RichPresence != null)
                this.RichPresence = new DiscordRichPresence(other.RichPresence);
            if(other.CustomStatus != null)
                this.CustomStatus = new DiscordCustomStatus(other.CustomStatus);
        }

        internal void UpdateWith(TransportActivity rawActivity)
        {
            this.Name = rawActivity?.Name;
            this.ActivityType = rawActivity != null ? rawActivity.ActivityType : ActivityType.Playing;
            this.StreamUrl = rawActivity?.StreamUrl;

            if (rawActivity?.IsRichPresence() == true && this.RichPresence != null)
                this.RichPresence.UpdateWith(rawActivity);
            else this.RichPresence = rawActivity?.IsRichPresence() == true ? new DiscordRichPresence(rawActivity) : null;

            if (rawActivity?.IsCustomStatus() == true && this.CustomStatus != null)
                this.CustomStatus.UpdateWith(rawActivity.State, rawActivity.Emoji);
            else this.CustomStatus = rawActivity?.IsCustomStatus() == true
                ? new DiscordCustomStatus
                {
                    Name = rawActivity.State,
                    Emoji = rawActivity.Emoji
                }
                : null;
        }
    }

    /// <summary>
    /// Represents details for a custom status activity, attached to a <see cref="DiscordActivity"/>.
    /// </summary>
    public sealed class DiscordCustomStatus
    {
        /// <summary>
        /// Gets the name of this custom status.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the emoji of this custom status, if any.
        /// </summary>
        public DiscordEmoji Emoji { get; internal set; }

        internal DiscordCustomStatus() { }

        internal DiscordCustomStatus(DiscordCustomStatus other)
        {
            this.Name = other.Name;
            this.Emoji = other.Emoji;
        }

        internal void UpdateWith(string state, DiscordEmoji emoji)
        {
            this.Name = state;
            this.Emoji = emoji;
        }
    }

    /// <summary>
    /// Represents details for Discord rich presence, attached to a <see cref="DiscordActivity"/>.
    /// </summary>
    public sealed class DiscordRichPresence
    {
        /// <summary>
        /// Gets the details of this presence.
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
        /// Gets the instance status.
        /// </summary>
        public bool? Instance { get; internal set; }

        /// <summary>
        /// Gets the large image for the rich presence.
        /// </summary>
        public DiscordAsset LargeImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for large image.
        /// </summary>
        public string LargeImageText { get; internal set; }

        /// <summary>
        /// Gets the small image for the rich presence.
        /// </summary>
        public DiscordAsset SmallImage { get; internal set; }

        /// <summary>
        /// Gets the hovertext for small image.
        /// </summary>
        public string SmallImageText { get; internal set; }

        /// <summary>
        /// Gets the current party size.
        /// </summary>
        public long? CurrentPartySize { get; internal set; }

        /// <summary>
        /// Gets the maximum party size.
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

        /// <summary>
        /// Gets the buttons for the rich presence.
        /// </summary>
        public IReadOnlyList<string> Buttons { get; internal set; }

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
            this.Buttons = other.Buttons;
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
            this.Buttons = rawGame?.Buttons;

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
    public enum ActivityType
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
        Watching = 3,

        /// <summary>
        /// Indicates the current activity is a custom status.
        /// </summary>
        Custom = 4,

        /// <summary>
        /// Indicates the user is competing in something.
        /// </summary>
        Competing = 5
    }
}

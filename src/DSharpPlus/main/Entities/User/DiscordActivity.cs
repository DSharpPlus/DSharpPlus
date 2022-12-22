// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2022 DSharpPlus Contributors
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

namespace DSharpPlus.Entities;

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

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
        // Active sessions are indicated with an "online", "idle", or "dnd" string per platform. If a user is
        // offline or invisible, the corresponding field is not present.
        (reader.Value?.ToString().ToLowerInvariant()) switch // reader.Value can be a string, DateTime or DateTimeOffset (yes, it's weird)
        {
            "online" => UserStatus.Online,
            "idle" => UserStatus.Idle,
            "dnd" => UserStatus.DoNotDisturb,
            "invisible" => UserStatus.Invisible,
            _ => UserStatus.Offline,
        };

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
    public DiscordActivity() => ActivityType = ActivityType.Playing;

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
        if (type == ActivityType.Custom)
        {
            throw new InvalidOperationException("Bots cannot use a custom status.");
        }

        Name = name;
        ActivityType = type;
    }

    internal DiscordActivity(TransportActivity rawActivity) => UpdateWith(rawActivity);

    internal DiscordActivity(DiscordActivity other)
    {
        Name = other.Name;
        ActivityType = other.ActivityType;
        StreamUrl = other.StreamUrl;
        if (other.RichPresence != null)
        {
            RichPresence = new DiscordRichPresence(other.RichPresence);
        }

        if (other.CustomStatus != null)
        {
            CustomStatus = new DiscordCustomStatus(other.CustomStatus);
        }
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
        else
        {
            RichPresence = rawActivity?.IsRichPresence() == true ? new DiscordRichPresence(rawActivity) : null;
        }

        if (rawActivity?.IsCustomStatus() == true && CustomStatus != null)
        {
            CustomStatus.UpdateWith(rawActivity.State, rawActivity.Emoji);
        }
        else
        {
            CustomStatus = rawActivity?.IsCustomStatus() == true
            ? new DiscordCustomStatus
            {
                Name = rawActivity.State,
                Emoji = rawActivity.Emoji
            }
            : null;
        }
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
        Name = other.Name;
        Emoji = other.Emoji;
    }

    internal void UpdateWith(string state, DiscordEmoji emoji)
    {
        Name = state;
        Emoji = emoji;
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

    internal DiscordRichPresence(TransportActivity rawGame) => UpdateWith(rawGame);

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
        Buttons = other.Buttons;
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
        if (rawGame?.Party != null && ulong.TryParse(rawGame.Party.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong partyId))
        {
            PartyId = partyId;
        }

        StartTimestamp = rawGame?.Timestamps?.Start;
        EndTimestamp = rawGame?.Timestamps?.End;
        JoinSecret = rawGame?.Secrets?.Join;
        MatchSecret = rawGame?.Secrets?.Match;
        SpectateSecret = rawGame?.Secrets?.Spectate;
        Buttons = rawGame?.Buttons;

        string? lid = rawGame?.Assets?.LargeImage;
        if (lid != null)
        {
            if (lid.StartsWith("spotify:"))
            {
                LargeImage = new DiscordSpotifyAsset { Id = lid };
            }
            else if (ulong.TryParse(lid, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong ulid))
            {
                LargeImage = new DiscordApplicationAsset { Id = lid, Application = Application, Type = ApplicationAssetType.LargeImage };
            }
        }

        string? sid = rawGame?.Assets?.SmallImage;
        if (sid != null)
        {
            if (sid.StartsWith("spotify:"))
            {
                SmallImage = new DiscordSpotifyAsset { Id = sid };
            }
            else if (ulong.TryParse(sid, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong usid))
            {
                SmallImage = new DiscordApplicationAsset { Id = sid, Application = Application, Type = ApplicationAssetType.LargeImage };
            }
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

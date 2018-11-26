﻿using DSharpPlus.Net.Abstractions;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Threading.Tasks;

#if WINDOWS_UWP
using Windows.UI.Xaml.Media;
using Windows.UI;
using Media = Windows.UI;
#elif WINDOWS_WPF
using System.Windows.Media;
using Media = System.Windows.Media;
#endif

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents a Discord user.
    /// </summary>
    public class DiscordUser : SnowflakeObject, IEquatable<DiscordUser>
    {
        private string _username;
        private string _discriminator;
        private string _avatarHash;

        internal DiscordUser() { }
        internal DiscordUser(TransportUser transport)
        {
            Id = transport.Id;
            Username = transport.Username;
            Discriminator = transport.Discriminator;
            AvatarHash = transport.AvatarHash;
            IsBot = transport.IsBot;
            MfaEnabled = transport.MfaEnabled;
            Verified = transport.Verified;
            Email = transport.Email;
        }

        /// <summary>
        /// Gets this user's username.
        /// </summary>
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Username
        {
            get => _username;
            internal set
            {
                OnPropertySet(ref _username, value);
                InvokePropertyChanged(nameof(DisplayName));
            }
        }

        public virtual string DisplayName => Username;

        /// <summary>
        /// Gets the user's 4-digit discriminator.
        /// </summary>
        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Discriminator
        {
            get => _discriminator;
            internal set
            {
                OnPropertySet(ref _discriminator, value);
                InvokePropertyChanged(nameof(DiscriminatorInt));
                InvokePropertyChanged(nameof(DefaultAvatarUrl));
            }
        }

        [JsonIgnore]
        internal int DiscriminatorInt
            => int.Parse(Discriminator, NumberStyles.Integer, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the user's avatar hash.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string AvatarHash
        {
            get => _avatarHash;
            internal set
            {
                OnPropertySet(ref _avatarHash, value);
                InvokePropertyChanged(nameof(AvatarUrl));
                InvokePropertyChanged(nameof(NonAnimatedAvatarUrl));
            }
        }

#if WINDOWS_UWP || WINDOWS_WPF
        [JsonIgnore]
        public virtual SolidColorBrush ColorBrush => null;
#endif

        /// <summary>
        /// Gets the user's avatar URL.
        /// </summary>
        [JsonIgnore]
        public string AvatarUrl
            => !string.IsNullOrWhiteSpace(AvatarHash) ? (AvatarHash.StartsWith("a_") ? $"https://cdn.discordapp.com/avatars/{Id.ToString(CultureInfo.InvariantCulture)}/{AvatarHash}.gif?size=128" : $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png?size=128") : DefaultAvatarUrl;

        /// <summary>
        /// Gets the user's avatar URL.
        /// </summary>
        [JsonIgnore]
        public string NonAnimatedAvatarUrl
            => !string.IsNullOrWhiteSpace(AvatarHash) ? $"https://cdn.discordapp.com/avatars/{Id}/{AvatarHash}.png?size=128" : DefaultAvatarUrl;
        
        /// <summary>
        /// Gets the URL of default avatar for this user.
        /// </summary>
        [JsonIgnore]
        public string DefaultAvatarUrl
            => $"https://cdn.discordapp.com/embed/avatars/{(DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture)}.png?size=128";

        /// <summary>
        /// Gets whether the user is a bot.
        /// </summary>
        [JsonProperty("bot", NullValueHandling = NullValueHandling.Ignore)]
        public virtual bool IsBot { get; internal set; }

        /// <summary>
        /// Does the current user have Discord Nitro
        /// </summary>
        [JsonIgnore]
        public bool HasNitro { get; internal set; }

        /// <summary>
        /// Gets whether the user has multi-factor authentication enabled.
        /// </summary>
        [JsonProperty("mfa_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public virtual bool? MfaEnabled { get; internal set; }

        /// <summary>
        /// Gets whether the user is verified.
        /// </summary>
        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public virtual bool? Verified { get; internal set; }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Email { get; internal set; }

        /// <summary>
        /// Gets the user's mention string.
        /// </summary>
        [JsonIgnore]
        public string Mention
            => Formatter.Mention(this, this is DiscordMember);

        /// <summary>
        /// Gets whether this user is the Client which created this object.
        /// </summary>
        [JsonIgnore]
        public bool IsCurrent
            => Id == Discord.CurrentUser.Id;

        /// <summary>
        /// Unbans this user from a guild.
        /// </summary>
        /// <param name="guild">Guild to unban this user from.</param>
        /// <param name="reason">Reason for audit logs.</param>
        /// <returns></returns>
        public Task UnbanAsync(DiscordGuild guild, string reason = null)
            => guild.UnbanMemberAsync(this, reason);

        /// <summary>
        /// Gets this user's presence.
        /// </summary>
        [JsonIgnore]
        public DiscordPresence Presence
        {
            get
            {
                if (Discord is DiscordClient dc)
                {
                    return dc.Presences.TryGetValue(Id, out var p) ? p : null;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the user's avatar URL, in requested format and size.
        /// </summary>
        /// <param name="fmt">Format of the avatar to get.</param>
        /// <param name="size">Maximum size of the avatar. Must be a power of two, minimum 16, maximum 2048.</param>
        /// <returns>URL of the user's avatar.</returns>
        public string GetAvatarUrl(ImageFormat fmt, ushort size = 1024)
        {
            if (fmt == ImageFormat.Unknown)
            {
                throw new ArgumentException("You must specify valid image format.", nameof(fmt));
            }

            if (size < 16 || size > 2048)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            var log = Math.Log(size, 2);
            if (log < 4 || log > 11 || log % 1 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            var sfmt = "";
            switch (fmt)
            {
                case ImageFormat.Gif:
                    sfmt = "gif";
                    break;

                case ImageFormat.Jpeg:
                    sfmt = "jpg";
                    break;

                case ImageFormat.Png:
                    sfmt = "png";
                    break;

                case ImageFormat.WebP:
                    sfmt = "webp";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(fmt));
            }

            var ssize = size.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(AvatarHash))
            {
                var id = Id.ToString(CultureInfo.InvariantCulture);
                return $"https://cdn.discordapp.com/avatars/{id}/{AvatarHash}.{sfmt}?size={ssize}";
            }
            else
            {
                var type = (DiscriminatorInt % 5).ToString(CultureInfo.InvariantCulture);
                return $"https://cdn.discordapp.com/embed/avatars/{type}.{sfmt}?size={ssize}";
            }
        }

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        public Task<DiscordDmChannel> CreateDmChannelAsync()
            => Discord.ApiClient.CreateDmAsync(Id);

        /// <summary>
        /// Returns a string representation of this user.
        /// </summary>
        /// <returns>String representation of this user.</returns>
        public override string ToString() => $"User {Id}; {Username}#{Discriminator}";

        /// <summary>
        /// Checks whether this <see cref="DiscordUser"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordUser"/>.</returns>
        public override bool Equals(object obj) => Equals(obj as DiscordUser);

        /// <summary>
        /// Checks whether this <see cref="DiscordUser"/> is equal to another <see cref="DiscordUser"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordUser"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordUser"/> is equal to this <see cref="DiscordUser"/>.</returns>
        public bool Equals(DiscordUser e)
        {
            if (ReferenceEquals(e, null))
            {
                return false;
            }

            if (ReferenceEquals(this, e))
            {
                return true;
            }

            return Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordUser"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordUser"/>.</returns>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordUser"/> objects are equal.
        /// </summary>
        /// <param name="e1">First user to compare.</param>
        /// <param name="e2">Second user to compare.</param>
        /// <returns>Whether the two users are equal.</returns>
        public static bool operator ==(DiscordUser e1, DiscordUser e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
            {
                return false;
            }

            if (o1 == null && o2 == null)
            {
                return true;
            }

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordUser"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First user to compare.</param>
        /// <param name="e2">Second user to compare.</param>
        /// <returns>Whether the two users are not equal.</returns>
        public static bool operator !=(DiscordUser e1, DiscordUser e2)
            => !(e1 == e2);
    }
}

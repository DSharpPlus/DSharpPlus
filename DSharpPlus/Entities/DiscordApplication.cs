using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;

namespace DSharpPlus.Entities
{
    /// <summary>
    /// Represents an OAuth2 application.
    /// </summary>
    public sealed class DiscordApplication : SnowflakeObject, IEquatable<DiscordApplication>
    {
        /// <summary>
        /// Gets the application's description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the application's icon.
        /// </summary>
        public string Icon => !string.IsNullOrWhiteSpace(this.IconHash) ? $"https://cdn.discordapp.com/app-icons/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=1024" : null;
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        internal string IconHash { get; set; }

        /// <summary>
        /// Gets the application's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the application's allowed RPC origins.
        /// </summary>
        [JsonProperty("rpc_origins", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> RpcOrigins { get; internal set; }

        /// <summary>
        /// Gets the application's flags.
        /// </summary>
        [JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
        public int Flags { get; internal set; }

        /// <summary>
        /// Gets the application's owner.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public DiscordUser Owner { get; internal set; }

        [JsonIgnore]
        private IReadOnlyList<DiscordApplicationAsset> Assets { get; set; }

        internal DiscordApplication() { }

        /// <summary>
        /// Retrieves this application's assets.
        /// </summary>
        /// <returns>This application's assets.</returns>
        public async Task<IReadOnlyList<DiscordApplicationAsset>> GetAssetsAsync()
        {
            if (this.Assets == null)
                this.Assets = await this.Discord.ApiClient.GetApplicationAssetsAsync(this);

            return this.Assets;
        }

        public string GenerateBotOAuth(Permissions permissions = Permissions.None)
        {
            permissions &= PermissionMethods.FULL_PERMS;
            // Split it up so it isn't annoying and blue
            // 
            // :blobthonkang: -emzi
            return "https://" + $"discordapp.com/oauth2/authorize?client_id={this.Id.ToString(CultureInfo.InvariantCulture)}&scope=bot&permissions={((long)permissions).ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplication"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordApplication"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordApplication);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplication"/> is equal to another <see cref="DiscordApplication"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordApplication"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordApplication"/> is equal to this <see cref="DiscordApplication"/>.</returns>
        public bool Equals(DiscordApplication e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordApplication"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplication"/> objects are equal.
        /// </summary>
        /// <param name="e1">First application to compare.</param>
        /// <param name="e2">Second application to compare.</param>
        /// <returns>Whether the two applications are equal.</returns>
        public static bool operator ==(DiscordApplication e1, DiscordApplication e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplication"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First application to compare.</param>
        /// <param name="e2">Second application to compare.</param>
        /// <returns>Whether the two applications are not equal.</returns>
        public static bool operator !=(DiscordApplication e1, DiscordApplication e2) =>
            !(e1 == e2);
    }

    /// <summary>
    /// Represents an asset for an OAuth2 application.
    /// </summary>
    public sealed class DiscordApplicationAsset : SnowflakeObject, IEquatable<DiscordApplicationAsset>
    {
        /// <summary>
        /// Gets the asset's name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the asset's type.
        /// </summary>
        [JsonProperty("type")]
        public ApplicationAssetType Type { get; internal set; }

        /// <summary>
        /// Gets the application this asset belongs to.
        /// </summary>
        public DiscordApplication Application { get; internal set; }

        /// <summary>
        /// Gets the Url of this asset.
        /// </summary>
        public Uri Url => new Uri($"https://cdn.discordapp.com/app-assets/{this.Application.Id.ToString(CultureInfo.InvariantCulture)}/{this.Id}.png");

        internal DiscordApplicationAsset() { }

        internal DiscordApplicationAsset(DiscordApplication app)
        {
            this.Discord = app.Discord;
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DiscordApplicationAsset);
        }

        /// <summary>
        /// Checks whether this <see cref="DiscordApplicationAsset"/> is equal to another <see cref="DiscordApplicationAsset"/>.
        /// </summary>
        /// <param name="e"><see cref="DiscordApplicationAsset"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordApplicationAsset"/> is equal to this <see cref="DiscordApplicationAsset"/>.</returns>
        public bool Equals(DiscordApplicationAsset e)
        {
            if (ReferenceEquals(e, null))
                return false;

            if (ReferenceEquals(this, e))
                return true;

            return this.Id == e.Id;
        }

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordApplication"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordApplication"/>.</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are equal.
        /// </summary>
        /// <param name="e1">First application asset to compare.</param>
        /// <param name="e2">Second application asset to compare.</param>
        /// <returns>Whether the two application assets not equal.</returns>
        public static bool operator ==(DiscordApplicationAsset e1, DiscordApplicationAsset e2)
        {
            var o1 = e1 as object;
            var o2 = e2 as object;

            if ((o1 == null && o2 != null) || (o1 != null && o2 == null))
                return false;

            if (o1 == null && o2 == null)
                return true;

            return e1.Id == e2.Id;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordApplicationAsset"/> objects are not equal.
        /// </summary>
        /// <param name="e1">First application asset to compare.</param>
        /// <param name="e2">Second application asset to compare.</param>
        /// <returns>Whether the two application assets are not equal.</returns>
        public static bool operator !=(DiscordApplicationAsset e1, DiscordApplicationAsset e2) =>
            !(e1 == e2);
    }

    /// <summary>
    /// Determines the type of the asset attached to the application.
    /// </summary>
    public enum ApplicationAssetType : int
    {
        /// <summary>
        /// Unknown type. This indicates something went terribly wrong.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// This asset can be used as small image for rich presences.
        /// </summary>
        SmallImage = 1,

        /// <summary>
        /// This asset can be used as large image for rich presences.
        /// </summary>
        LargeImage = 2
    }
}

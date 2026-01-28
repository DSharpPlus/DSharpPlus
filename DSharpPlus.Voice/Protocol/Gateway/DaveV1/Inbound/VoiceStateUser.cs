
using System.Text.Json.Serialization;

public class VoiceStateUser
{
    /// <summary>
    /// Gets/Sets the users id
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }
    /// <summary>
    /// Gets/Sets the username
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
    /// <summary>
    /// Gets/Sets the discriminator
    /// </summary>
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }
    /// <summary>
    /// Gets/Sets the global name of the user
    /// </summary>
    [JsonPropertyName("global_name")]
    public string GlobalName { get; set; }
    /// <summary>
    /// Gets/Sets the display name of the user
    /// </summary>
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
    /// <summary>
    /// Gets/Sets the display name style
    /// </summary>
    [JsonPropertyName("display_name_styles")]
    public string DisplayNameStyles { get; set; }
    /// <summary>
    /// Gets/Sets the primary guild
    /// </summary>
    [JsonPropertyName("primary_guild")]
    public string PrimaryGuild { get; set; }
    /// <summary>
    /// Gets/Sets the public flags
    /// </summary>
    [JsonPropertyName("public_flags")]
    public int PublicFlags { get; set; }
    /// <summary>
    /// Gets/Sets the collectibles
    /// </summary>
    [JsonPropertyName("collectibles")]
    public string Collectibles { get; set; }
    /// <summary>
    /// Gets/Sets if the user is a bot
    /// </summary>
    [JsonPropertyName("bot")]
    public bool Bot { get; set; }
    /// <summary>
    /// Gets/Sets the avatar decoration data
    /// </summary>
    [JsonPropertyName("avatar_decoration_data")]
    public string AvatarDecorationData { get; set; }
    /// <summary>
    /// Gets/Sets the users avatar
    /// </summary>
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}

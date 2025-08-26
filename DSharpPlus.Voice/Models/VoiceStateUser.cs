// See https://aka.ms/new-console-template for more information
using System.Text.Json.Serialization;

public class VoiceStateUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; }

    [JsonPropertyName("global_name")]
    public string GlobalName { get; set; }

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }

    [JsonPropertyName("display_name_styles")]
    public string DisplayNameStyles { get; set; }

    [JsonPropertyName("primary_guild")]
    public string PrimaryGuild { get; set; }

    [JsonPropertyName("public_flags")]
    public int PublicFlags { get; set; }

    [JsonPropertyName("collectibles")]
    public string Collectibles { get; set; }

    [JsonPropertyName("bot")]
    public bool Bot { get; set; }

    [JsonPropertyName("avatar_decoration_data")]
    public string AvatarDecorationData { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
}

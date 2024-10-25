namespace DSharpPlus.Entities;

/// <summary>
/// Represents the type of the webhook event body.
/// </summary>
public enum DiscordWebhookEventBodyType
{
    /// <summary>
    /// Represents that the application was authorized to a user or guild.
    /// </summary>
    ApplicationAuthorized,

    /// <summary>
    /// Represents that an entitlement was created.
    /// </summary>
    EntitlementCreate,

    /// <summary>
    /// Represents that a user was enrolled in a quest.
    /// </summary>
    /// <remarks>
    /// The details of this are currently undocumented, and thus this value only exists for parity with Discord's documentation.
    /// </remarks>
    QuestUserEnrollment,
}

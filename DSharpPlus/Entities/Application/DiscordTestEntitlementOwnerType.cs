namespace DSharpPlus.Entities;

/// <summary>
/// Defines what type of owner the test entitlement should have
/// </summary>
public enum DiscordTestEntitlementOwnerType
{
    /// <summary>
    /// The test entitlement should belong to a guild
    /// </summary>
    Guild = 1,
    
    /// <summary>
    /// The test entitlement should belong to a user
    /// </summary>
    User = 2
}

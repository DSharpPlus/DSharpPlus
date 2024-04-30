namespace DSharpPlus.Entities;

/// <summary>
/// Represents the possible OAuth scopes for application authorization.
/// </summary>
public enum DiscordOAuthScope
{
    /// <summary>
    /// Allows <c>/users/@me</c> without <c>email</c>.
    /// </summary>
    Identify,

    /// <summary>
    /// Enables <c>/users/@me</c> to return <c>email</c>.
    /// </summary>
    Email,

    /// <summary>
    /// Allows <c>/users/@me/connections</c> to return linked third-party accounts.
    /// </summary>
    Connections,

    /// <summary>
    /// Allows <c>/users/@me/guilds</c> to return basic information about all of a user's guilds.
    /// </summary>
    Guilds,

    /// <summary>
    /// Allows <c>/guilds/{guild.id}/members/{user.id}</c> to be used for joining users into a guild.
    /// </summary>
    GuildsJoin,

    /// <summary>
    /// Allows <c>/users/@me/guilds/{guild.id}/members</c> to return a user's member information in a guild.
    /// </summary>
    GuildsMembersRead,

    /// <summary>
    /// Allows your app to join users into a group DM.
    /// </summary>
    GdmJoin,

    /// <summary>
    /// For local RPC server access, this allows you to control a user's local Discord client.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    Rpc,

    /// <summary>
    /// For local RPC server access, this allows you to receive notifications pushed to the user.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcNotificationsRead,

    /// <summary>
    /// For local RPC server access, this allows you to read a user's voice settings and listen for voice events.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcVoiceRead,

    /// <summary>
    /// For local RPC server access, this allows you to update a user's voice settings.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcVoiceWrite,

    /// <summary>
    /// For local RPC server access, this allows you to update a user's activity.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RpcActivitiesWrite,

    /// <summary>
    /// For OAuth2 bots, this puts the bot in the user's selected guild by default.
    /// </summary>
    Bot,

    /// <summary>
    /// This generates a webhook that is returned in the OAuth token response for authorization code grants.
    /// </summary>
    WebhookIncoming,

    /// <summary>
    /// For local RPC server access, this allows you to read messages from all client channels
    /// (otherwise restricted to channels/guilds your application creates).
    /// </summary>
    MessagesRead,

    /// <summary>
    /// Allows your application to upload/update builds for a user's applications.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    ApplicationsBuildsUpload,

    /// <summary>
    /// Allows your application to read build data for a user's applications.
    /// </summary>
    ApplicationsBuildsRead,

    /// <summary>
    /// Allows your application to use application commands in a guild.
    /// </summary>
    ApplicationsCommands,

    /// <summary>
    /// Allows your application to read and update store data (SKUs, store listings, achievements etc.) for a user's applications.
    /// </summary>
    ApplicationsStoreUpdate,

    /// <summary>
    /// Allows your application to read entitlements for a user's applications.
    /// </summary>
    ApplicationsEntitlements,

    /// <summary>
    /// Allows your application to fetch data from a user's "Now Playing/Recently Played" list.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    ActivitiesRead,

    /// <summary>
    /// Allows your application to update a user's activity.
    /// </summary>
    /// <remarks>
    /// Outside of the GameSDK activity manager, this scope requires Discord approval.
    /// </remarks>
    ActivitiesWrite,

    /// <summary>
    /// Allows your application to know a user's friends and implicit relationships.
    /// </summary>
    /// <remarks>
    /// This scope requires Discord approval.
    /// </remarks>
    RelationshipsRead
}

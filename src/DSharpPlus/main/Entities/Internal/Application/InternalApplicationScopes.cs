namespace DSharpPlus.Entities.Internal;

public static class InternalApplicationScopes
{
    /// <summary>
    /// Allows your app to fetch data from a user's "Now Playing/Recently Played" list - requires Discord approval.
    /// </summary>
    public const string ActivitiesRead = "activities.read";

    /// <summary>
    /// Allows your app to update a user's activity - requires Discord approval (NOT REQUIRED FOR 
    /// <see href="https://discord.com/developers/docs/game-sdk/activities">GAMESDK ACTIVITY MANAGER</see>).
    /// </summary>
    public const string ActivitiesWrite = "activities.write";

    /// <summary>
    /// Allows your app to read build data for a user's applications.
    /// </summary>
    public const string ApplicationsBuildsRead = "applications.builds.read";

    /// <summary>
    /// Allows your app to upload/update builds for a user's applications - requires Discord approval.
    /// </summary>
    public const string ApplicationsBuildsUpload = "applications.builds.upload";

    /// <summary>
    /// Allows your app to use <seealso cref="InternalApplicationCommand"/>s in a guild.
    /// </summary>
    public const string ApplicationsCommands = "applications.commands";

    /// <summary>
    /// Allows your app to update its <seealso cref="InternalApplicationCommand"/>s using a Bearer token - 
    /// client credentials grant only.
    /// </summary>
    public const string ApplicationsCommandsUpdate = "applications.commands.update";

    /// <summary>
    /// Allows your app to update application command permissions in a guild a user has permissions to.
    /// </summary>
    public const string ApplicationsCommandsPermissionsUpdate = "applications.commands.permissions.update";

    /// <summary>
    /// Allows your app to read entitlements for a user's applications.
    /// </summary>
    public const string ApplicationsEntitlements = "applications.entitlements";

    /// <summary>
    /// Allows your app to read and update store data (SKUs, store listings, achievements, etc.) for a user's applications.
    /// </summary>
    public const string ApplicationsStoreUpdate = "applications.store.update";

    /// <summary>
    /// For oauth2 bots, this puts the bot in the user's selected guild by default.
    /// </summary>
    /// <remarks>
    /// This requires you to have a bot account linked to your application.
    /// </remarks>
    public const string Bot = "bot";

    /// <summary>
    /// Allows <see href="https://discord.com/developers/docs/resources/user#get-user-connections">/users/@me/connections</see> to return linked third-party accounts.
    /// </summary>
    public const string Connections = "connections";

    /// <summary>
    /// Allows your app to see information about the user's DMs and group DMs - requires Discord approval.
    /// </summary>
    public const string DmChannelsRead = "dm_channels.read";

    /// <summary>
    /// Enables <see href="https://discord.com/developers/docs/resources/user#get-current-user">/users/@me</see> to return an email.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// Allows your app to <see href="https://discord.com/developers/docs/resources/channel#group-dm-add-recipient">join users to a group dm</see>.
    /// </summary>
    public const string GdmJoin = "gdm.join";

    /// <summary>
    /// Allows <see href="https://discord.com/developers/docs/resources/user#get-current-user-guilds">/users/@me/guilds</see> to return basic information about all of a user's guilds.
    /// </summary>
    public const string Guilds = "guilds";

    /// <summary>
    /// Allows <see href="https://discord.com/developers/docs/resources/guild#add-guild-member">/guilds/{guild.id}/members/{user.id}</see> to be used for joining users to a guild
    /// </summary>
    /// <remarks>
    /// This requires you to have a bot account linked to your application. Also, in order to add a user to a guild, your bot has to already belong to that guild.
    /// </remarks>
    public const string GuildsJoin = "guilds.join";

    /// <summary>
    /// Allows <see href="https://discord.com/developers/docs/resources/user#get-current-user-guild-member">/users/@me/guilds/{guild.id}/member</see> to return a user's member information in a guild
    /// </summary>
    public const string GuildsMembersRead = "guilds.members.read";

    /// <summary>
    /// Allows <see href="https://discord.com/developers/docs/resources/user#get-current-user">/users/@me</see> without email.
    /// </summary>
    public const string Identify = "identify";

    /// <summary>
    /// For local rpc server api access, this allows you to read messages from all client channels (otherwise restricted to channels/guilds your app creates).
    /// </summary>
    public const string MessagesRead = "messages.read";

    /// <summary>
    /// Allows your app to know a user's friends and implicit relationships - requires Discord approval.
    /// </summary>
    public const string RelationshipsRead = "relationships.read";

    /// <summary>
    /// For local rpc server access, this allows you to control a user's local Discord client - requires Discord approval.
    /// </summary>
    public const string Rpc = "rpc";

    /// <summary>
    /// For local rpc server access, this allows you to update a user's activity - requires Discord approval.
    /// </summary>
    public const string RpcActivitiesWrite = "rpc.activities.write";

    /// <summary>
    /// For local rpc server access, this allows you to receive notifications pushed out to the user - requires Discord approval.
    /// </summary>
    public const string RpcNotificationsRead = "rpc.notifications.read";

    /// <summary>
    /// For local rpc server access, this allows you to read a user's voice settings and listen for voice events - requires Discord approval.
    /// </summary>
    public const string RpcVoiceRead = "rpc.voice.read";

    /// <summary>
    /// For local rpc server access, this allows you to update a user's voice settings - requires Discord approval.
    /// </summary>
    public const string RpcVoiceWrite = "rpc.voice.write";

    /// <summary>
    /// Allows your app to connect to voice on user's behalf and see all the voice members - requires Discord approval.
    /// </summary>
    public const string Voice = "voice";

    /// <summary>
    /// This generates a webhook that is returned in the oauth token response for authorization code grants.
    /// </summary>
    public const string WebhookIncoming = "webhook.incoming";
}

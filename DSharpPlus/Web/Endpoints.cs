namespace DSharpPlus
{
    internal static class Endpoints
    {
        private const string INTERNAL_BASE_URI = "https://{0}discordapp.com/api/v7";
        public static string BaseUriCanary => string.Format(INTERNAL_BASE_URI, "canary.");
        public static string BaseUriPTB => string.Format(INTERNAL_BASE_URI, "ptb.");
        public static string BaseUriStable => string.Format(INTERNAL_BASE_URI, "");

        public const string OAUTH2 = "/oauth2";
        public const string APPLICATIONS = "/applications";
        public const string REACTIONS = "/reactions";
        public const string ME = "/@me";
        public const string PERMISSIONS = "/permissions";
        public const string RECIPIENTS = "/recipients";
        public const string BULK_DELETE = "/bulk-delete";
        public const string EMBED = "/embed";
        public const string INTEGRATIONS = "/integrations";
        public const string SYNC = "/sync";
        public const string PRUNE = "/prune";
        public const string REGIONS = "/regions";
        public const string CONNECTIONS = "/connections";
        public const string ICONS = "/icons";
        public const string GATEWAY = "/gateway";
        public const string AUTH = "/auth";
        public const string LOGIN = "/login";
        public const string CHANNELS = "/channels";
        public const string MESSAGES = "/messages";
        public const string PINS = "/pins";
        public const string USERS = "/users";
        public const string GUILDS = "/guilds";
        public const string INVITES = "/invites";
        public const string INVITE = "/invite";
        public const string ROLES = "/roles";
        public const string MEMBERS = "/members";
        public const string TYPING = "/typing";
        public const string AVATARS = "/avatars";
        public const string BANS = "/bans";
        public const string WEBHOOKS = "/webhooks";
        public const string SLACK = "/slack";
        public const string GITHUB = "/github";
        public const string BOT = "/bot";
        public const string VOICE = "/voice";
        public const string AUDIT_LOGS = "/audit-logs";
        public const string ACK = "/ack";
        public const string NICK = "/nick";
        public const string ASSETS = "/assets";
    }
}

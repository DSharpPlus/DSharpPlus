namespace DSharpPlus
{
    internal static class Endpoints
    {
        private const string InternalBaseUri = "https://{0}discordapp.com/api";
        public static string CanaryBaseUri => string.Format(InternalBaseUri, "canary.");
        public static string PTBBaseUri => string.Format(InternalBaseUri, "ptb.");
        public static string StableBaseUri => string.Format(InternalBaseUri, "");

        public const string Reactions = "/reactions";
        public const string me = "/@me";
        public const string Permissions = "/permissions";
        public const string Recipients = "/recipients";
        public const string BulkDelete = "/bulk-delete";
        public const string Embed = "/embed";
        public const string Integrations = "/integrations";
        public const string Sync = "/sync";
        public const string Prune = "/prune";
        public const string Regions = "/regions";
        public const string Connections = "/connections";
        public const string Icons = "/icons";
        public const string Gateway = "/gateway";
        public const string Auth = "/auth";
        public const string Login = "/login";
        public const string Channels = "/channels";
        public const string Messages = "/messages";
        public const string Pins = "/pins";
        public const string Users = "/users";
        public const string Guilds = "/guilds";
        public const string Invites = "/invites";
        public const string Invite = "/invite";
        public const string Roles = "/roles";
        public const string Members = "/members";
        public const string Typing = "/typing";
        public const string Avatars = "/avatars";
        public const string Bans = "/bans";
        public const string Webhooks = "/webhooks";
        public const string Slack = "/slack";
        public const string Github = "/github";
        public const string Bot = "/bot";
        public const string Voice = "/voice";
    }
}

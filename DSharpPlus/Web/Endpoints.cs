namespace DSharpPlus
{
    public static class Endpoints
    {
        private static string InternalBaseUri => "https://{0}discordapp.com/api";
        public static string CanaryBaseUri => string.Format(InternalBaseUri, "canary.");
        public static string PTBBaseUri => string.Format(InternalBaseUri, "ptb.");
        public static string StableBaseUri => string.Format(InternalBaseUri, "");

        public static string Users => "/users";
        public static string Guilds => "/guilds";
        public static string Channels => "/channels";
        public static string Members => "/members";
        public static string Gateway => "/gateway";
        public static string Bot => "/bot";
        public static string Messages => "/messages";
        public static string Bans => "/bans";
        public static string BulkDelete => "/bulk-delete";
        public static string Invites => "/invites";
        public static string Permissions => "/permissions";
        public static string Typing => "/typing";
        public static string Pins => "/pins";
        public static string Recipients => "/recipients";
        public static string Roles => "/roles";
        public static string Prune => "/prune";
        public static string Regions => "regions";
        public static string Sync => "sync";
        public static string Embed => "embed";
        public static string Integrations => "integrations";
    }
}

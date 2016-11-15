using System.Collections.Generic;

namespace DSharpPlus
{
    public class DiscordConnection : SnowflakeObject
    {
        public string Name { get; internal set; }
        public string Type { get; internal set; }
        public bool Revoked { get; internal set; }
        public List<DiscordIntegration> Integrations { get; internal set; }
    }
}

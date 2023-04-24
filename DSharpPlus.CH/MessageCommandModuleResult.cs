namespace DSharpPlus.CH
{
    public class MessageCommandModuleResult : IMessageCommandModuleResult
    {
        public MessageCommandModuleResultType Type { get; set; }
        public string? Content { get; set; }
        public List<DSharpPlus.Entities.DiscordEmbed>? Embeds { get; set; }

        public static implicit operator MessageCommandModuleResult(DSharpPlus.Entities.DiscordEmbed embed)
        {
            var msgCmdResult = new MessageCommandModuleResult();
            if (msgCmdResult.Embeds is null) msgCmdResult.Embeds = new List<Entities.DiscordEmbed> { embed };

            return msgCmdResult;
        }

        public static implicit operator MessageCommandModuleResult(DSharpPlus.Entities.DiscordEmbedBuilder builder)
        {
            var embed = builder.Build();
            var msgCmdResult = new MessageCommandModuleResult();
            if (msgCmdResult.Embeds is null) msgCmdResult.Embeds = new List<Entities.DiscordEmbed> { embed };

            return msgCmdResult;
        }

        public static implicit operator MessageCommandModuleResult(string str)
        {
            var msgCmdResult = new MessageCommandModuleResult();
            msgCmdResult.Content = str;
            return msgCmdResult;
        }
    }
}


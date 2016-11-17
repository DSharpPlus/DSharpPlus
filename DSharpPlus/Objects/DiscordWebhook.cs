using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class DiscordWebhook : SnowflakeObject
    {
        public ulong GuildID { get; internal set; }
        public ulong ChannelID { get; internal set; }
        public DiscordUser User { get; internal set; }
        public string Name { get; internal set; }
        public string Avatar { get; internal set; }
        public string Token { get; internal set; }

        public async Task<DiscordWebhook> Modify(string name = "", string base64avatar = "") => await DiscordClient.InternalModifyWebhook(ID, name, base64avatar, Token);
        public async Task Delete() => await DiscordClient.InternalDeleteWebhook(ID, Token);
        public async Task Execute(string content = "", string username = "", string avatarurl = "", bool tts = false, List<DiscordEmbed> embeds = null)
            => await DiscordClient.InternalExecuteWebhook(ID, Token, content, username, avatarurl, tts, embeds);
        public async Task ExecuteSlack(string Json) => await DiscordClient.InternalExecuteWebhookSlack(ID, Token, Json);
        public async Task ExecuteGithub(string Json) => await DiscordClient.InternalExecuteWebhookGithub(ID, Token, Json);
    }
}

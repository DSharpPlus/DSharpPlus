using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class MemberBuilderTests : BaseCommandModule
    {
        [Command]
        public async Task ModifyMember(CommandContext ctx, DiscordMember user)
        {
            await new DiscordMemberModifyBuilder()
                .WithNickname("DummyTester")
                .ModifyAysnc(user);
        }

        [Command]
        public async Task ModifyMemberAction(CommandContext ctx, DiscordMember user)
        {
            var vcahnnel = ctx.Guild.Channels.Where(x => x.Value.Type == ChannelType.Voice).FirstOrDefault();
            await user.ModifyAsync(m => {
                m.WithVoiceChannel(vcahnnel.Value)
                .WithDeafened(true)
                .WithMute(true);
            });

            await Task.Delay(5000);

            await user.ModifyAsync(m =>
            {
                m.WithVoiceChannel(null);
            });
        }
    }
}

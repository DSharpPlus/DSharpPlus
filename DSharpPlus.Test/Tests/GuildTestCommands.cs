using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DSharpPlus.Test.Tests
{
    public class GuildTestCommands : BaseCommandModule
    {
        [Command]
        public async Task CreateGuild(CommandContext ctx, [RemainingText]string name)
        {
            var guild = await ctx.Client.CreateGuildAsync(name);
            var channels = await guild.GetChannelsAsync();
            var invite = await channels.Where(x => !x.IsCategory && x.Type == ChannelType.Text).FirstOrDefault().CreateInviteAsync();
            await ctx.RespondAsync(invite.Code);
        }

        [Command]
        public async Task CreateGuildWithBuilder(CommandContext ctx, [RemainingText] string name)
        {
            try
            {
                var builder = new DiscordGuildBuilder()
                        .WithName(name)
                        .WithDefaultMessageNotificationLevel(DefaultMessageNotifications.MentionsOnly)
                        .WithExplicitContentFilterLevel(ExplicitContentFilter.MembersWithoutRoles)
                        .WithVerificationLevel(VerificationLevel.Low)
                        .WithRoles(new DiscordGuildBuilder.GuildBuilderRole[] {
                            new DiscordGuildBuilder.GuildBuilderRole{ Id = Convert.ToUInt64(000000000000000001), Mentionable = true, Name = "Everyone", Permissions = Permissions.Administrator },
                            new DiscordGuildBuilder.GuildBuilderRole{ Id = Convert.ToUInt64(000000000000000002), Mentionable = true, Name = "Role 1" },
                            new DiscordGuildBuilder.GuildBuilderRole{ Id = Convert.ToUInt64(000000000000000003), Mentionable = true, Name = "Role 2" },
                        })
                        .WithChannels(new DiscordGuildBuilder.GuildBuilderChannel[] {
                            new DiscordGuildBuilder.GuildBuilderChannel { Id = Convert.ToUInt64(000000000000000004), Name = "Glock General", Type = ChannelType.Category, PermissionOverwrites = new DiscordGuildBuilder.GuildBuilderChannel.ChannelOverwrite[]{ } },
                            new DiscordGuildBuilder.GuildBuilderChannel { Id = Convert.ToUInt64(000000000000000005), Name = "Glock text General", Type = ChannelType.Text, ParentId = Convert.ToUInt64(000000000000000004), Nsfw = true, Topic = "Some dumb topic", PermissionOverwrites = new DiscordGuildBuilder.GuildBuilderChannel.ChannelOverwrite[] {
                                new DiscordGuildBuilder.GuildBuilderChannel.ChannelOverwrite { Id = Convert.ToUInt64(000000000000000002), DenyPermissions = Permissions.All, AllowPermissions = Permissions.None },
                                new DiscordGuildBuilder.GuildBuilderChannel.ChannelOverwrite { Id = Convert.ToUInt64(000000000000000003), AllowPermissions = Permissions.All, DenyPermissions = Permissions.None }
                            }}
                        });
                        //.WithSystemChannelId(Convert.ToUInt64(000000000000000003));


                var guild = await builder.SendAsync(ctx.Client);

                var channels = await guild.GetChannelsAsync();
                var invite = await channels.Where(x => !x.IsCategory && x.Type == ChannelType.Text).FirstOrDefault().CreateInviteAsync();
                await ctx.RespondAsync(invite.Code);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [Command]
        public async Task ModifyGuild(CommandContext ctx)
        {
            try
            {
                var builder = new DiscordGuildBuilder()
                    .WithName("Glocks Awesome Test Guild")
                    .WithAuditLogReason("Cause i can")
                    .WithNewOwener(ctx.Member)
                    .ModifyAsync(ctx.Guild);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}

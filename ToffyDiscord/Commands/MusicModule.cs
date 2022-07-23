using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ToffyDiscord.Commands;

public class MusicModule : BaseCommandModule
{
    [Command]
    private async Task ConnectAsync(CommandContext ctx, DiscordChannel channel)
    {
        var lava = ctx.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        {
            await ctx.PromotionResponseAsync("З’єднання з Lavalink не встановлено");
            return;
        }

        var node = lava.ConnectedNodes.Values.First();

        if (channel.Type != ChannelType.Voice)
        {
            await ctx.PromotionResponseAsync("Недійсний голосовий канал.");
            return;
        }

        await node.ConnectAsync(channel);
    }

    [Command]
    public async Task DisconnectAsync(CommandContext ctx)
    {
        var lava = ctx.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        {
            await ctx.PromotionResponseAsync("З’єднання з Lavalink не встановлено");
            return;
        }

        var node = lava.ConnectedNodes.Values.First();
        var channel = ctx.Member.VoiceState.Channel;
        if (channel.Type != ChannelType.Voice)
        {
            await ctx.PromotionResponseAsync("Недійсний голосовий канал.");
            return;
        }

        var conn = node.GetGuildConnection(channel.Guild);

        if (conn == null)
        {
            await ctx.PromotionResponseAsync("З’єднання з Lavalink не встановлено");
            return;
        }

        await conn.DisconnectAsync();
    }


    [Command]
    public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
    {
        await this.ConnectAsync(ctx, ctx.Member.VoiceState.Channel);


        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.PromotionResponseAsync("Ви не в голосовому каналі.");
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        if (!node.IsConnected)
        {
            await node.ConnectAsync(ctx.Member.VoiceState.Channel);
        }

        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        var loadResult = await node.Rest.GetTracksAsync(search);

        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await ctx.PromotionResponseAsync($"Не вдалося виконати пошук для {search}.");
            return;
        }

        var track = loadResult.Tracks.First();

        await conn.PlayAsync(track);

        await ctx.PromotionResponseAsync($"Зараз грає {track.Title}!");
    }

    [Command]
    public async Task PauseAsync(CommandContext ctx)
    {
        if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        {
            await ctx.PromotionResponseAsync("Ви не в голосовому каналі.");
            return;
        }

        var lava = ctx.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        if (conn == null)
        {
            await ctx.PromotionResponseAsync("З’єднання з Lavalink не встановлено");
            return;
        }

        if (conn.CurrentState.CurrentTrack == null)
        {
            await ctx.PromotionResponseAsync("Немає запису.");
            return;
        }

        await conn.PauseAsync();
    }
}

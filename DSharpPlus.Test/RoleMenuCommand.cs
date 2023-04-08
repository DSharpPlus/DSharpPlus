using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.Test;

public class RoleMenuCommand : BaseCommandModule
{

    [Command("role_menu")]
    public async Task RoleMenuAsync(CommandContext ctx, string message, string emojis, [RemainingText] params DiscordRole[] roles)
    {
        IArgumentConverter<DiscordEmoji> converter = new DiscordEmojiConverter();
        string[] split = emojis.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        DiscordEmoji[] emojiArray = new DiscordEmoji[roles.Length];

        if (split.Length < roles.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(emojis), "You don't have enough emojis!");
        }

        if (roles.Length > 25)
        {
            throw new ArgumentOutOfRangeException(nameof(roles), "You can only have up to 25 roles per role menu!");
        }

        for (int i = 0; i < roles.Length; i++)
        {
            Optional<DiscordEmoji> e = await converter.ConvertAsync(split[i], ctx);
            if (!e.HasValue)
            {
                throw new ArgumentException($"I couldn't parse {split[i]}");
            }

            emojiArray[i] = e.Value;
        }

        IEnumerable<DiscordEmoji> unavailable = emojiArray.Where(e => !e.IsAvailable && e.Id != 0);

        if (unavailable.Any())
        {
            throw new ArgumentException($"One or more emojis is from a server I'm not in!\nNames: {string.Join(", ", unavailable.Select(u => u.GetDiscordName()))}");
        }

        List<DiscordComponent> buttons = new(5);
        List<List<(DiscordRole First, DiscordEmoji Second)>> chnk = roles.Zip(emojiArray).Chunk(5).OrderBy(l => l.Count).ToList();

        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithContent(message.Replace("\\n", "\n") + $"\n{string.Join('\n', chnk.SelectMany(c => c).Select(p => $"{p.Second} -> {p.First.Mention}"))}")
            .WithAllowedMentions(Mentions.None);

        foreach (List<(DiscordRole First, DiscordEmoji Second)>? chunklist in chnk)
        {
            foreach ((DiscordRole first, DiscordEmoji second) in chunklist)
            {
                if (first.Position >= ctx.Guild.CurrentMember.Hierarchy)
                {
                    throw new InvalidOperationException("Cannot assign role higher or equal to my own role!");
                }

                if (first.Position > ctx.Member.Hierarchy)
                {
                    throw new InvalidOperationException("Cannot assign role higher than your own!");
                }

                DiscordComponentEmoji e = new(second.Id);
                DiscordButtonComponent b = new(ButtonStyle.Success, $"{first.Mention}", "", emoji: e);
                buttons.Add(b);
            }
            builder.AddComponents(buttons.ToArray());
            buttons.Clear();
        }
        await builder.SendAsync(ctx.Channel);
    }
}

public static class ChunkExtension
{
    public static List<List<T>> Chunk<T>(this IEnumerable<T> data, int size) => data
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / size)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
}

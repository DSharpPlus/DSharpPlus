using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace DSharpPlus.Test
{
    public class RoleMenuCommand : BaseCommandModule
    {

        [Command]
        public async Task RoleMenu(CommandContext ctx, string message, string emojis, [RemainingText] params DiscordRole[] roles)
        {
            var converter = (IArgumentConverter<DiscordEmoji>)new DiscordEmojiConverter();
            var split = emojis.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var emojiArray = new DiscordEmoji[roles.Length];

            if (split.Length < roles.Length)
                throw new ArgumentOutOfRangeException(nameof(emojis), "You don't have enough emojis!");

            if (roles.Length > 25)
                throw new ArgumentOutOfRangeException(nameof(roles),"You can only have up to 25 roles per role menu!");

            for (var i = 0; i < roles.Length; i++)
            {
                var e = await converter.ConvertAsync(split[i], ctx);
                if (!e.HasValue)
                    throw new ArgumentException($"I couldn't parse {split[i]}");
                emojiArray[i] = e.Value;
            }

            var unavailable = emojiArray.Where(e => !e.IsAvailable && e.Id != 0);

            if (unavailable.Any())
                throw new ArgumentException($"One or more emojis is from a server I'm not in!\nNames: {string.Join(", ", unavailable.Select(u => u.GetDiscordName()))}");

            var buttons = new List<DiscordComponent>(5);
            var chnk = roles.Zip(emojiArray).Chunk(5).OrderBy(l => l.Count).ToList();

            var builder = new DiscordMessageBuilder()
                .WithContent(message.Replace("\\n", "\n") + $"\n{string.Join('\n', chnk.SelectMany(c => c).Select(p => $"{p.Second} -> {p.First.Mention}"))}")
                .WithAllowedMentions(Mentions.None);

            foreach (var chunklist in chnk)
            {
                foreach (var pair in chunklist)
                {
                    if (pair.First.Position >= ctx.Guild.CurrentMember.Hierarchy)
                         throw new InvalidOperationException("Cannot assign role higher or equal to my own role!");

                    if (pair.First.Position > ctx.Member.Hierarchy)
                        throw new InvalidOperationException("Cannot assign role higher than your own!");

                    var e = new DiscordComponentEmoji(pair.Second.Id);
                    var b = new DiscordButtonComponent(ButtonStyle.Success, $"{pair.First.Mention}", "", emoji: e);
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
}

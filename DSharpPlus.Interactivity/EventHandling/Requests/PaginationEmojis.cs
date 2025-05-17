using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace DSharpPlus.Interactivity;

public class PaginationEmojis
{
    public DiscordEmoji SkipLeft;
    public DiscordEmoji SkipRight;
    public DiscordEmoji Left;
    public DiscordEmoji Right;
    public DiscordEmoji Stop;

    public PaginationEmojis()
    {
        this.Left = DiscordEmoji.FromUnicode("◀");
        this.Right = DiscordEmoji.FromUnicode("▶");
        this.SkipLeft = DiscordEmoji.FromUnicode("⏮");
        this.SkipRight = DiscordEmoji.FromUnicode("⏭");
        this.Stop = DiscordEmoji.FromUnicode("⏹");
    }
}

public class Page
{
    public string Content { get; set; }
    public DiscordEmbed Embed { get; set; }

    public IReadOnlyList<DiscordActionRowComponent> Components { get; }

    public Page(string content = "", DiscordEmbed? embed = null, IReadOnlyList<DiscordComponent> components = null)
    {
        this.Content = content;
        this.Embed = embed;

        if (components is null or [])
        {
            this.Components = [];

            return;
        }

        if (components[0] is DiscordActionRowComponent arc)
        {
            if (components.Count > 4)
            {
                throw new ArgumentException("Pages can only contain four rows of components");
            }

            this.Components = [arc];
        }
        else
        {
            List<DiscordActionRowComponent> componentRows = [];
            List<DiscordComponent> currentRow = new(5);

            foreach (DiscordComponent component in components)
            {
                if (component is BaseDiscordSelectComponent)
                {
                    componentRows.Add(new([component]));

                    continue;
                }

                if (currentRow.Count == 5)
                {
                    componentRows.Add(new DiscordActionRowComponent(currentRow));
                    currentRow = new List<DiscordComponent>(5);
                }

                currentRow.Add(component);
            }

            if (currentRow.Count > 0)
            {
                componentRows.Add(new DiscordActionRowComponent(currentRow));
            }

            this.Components = componentRows;

        }
    }
}

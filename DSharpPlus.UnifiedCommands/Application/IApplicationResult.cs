using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Application;

public interface IApplicationResult
{
    public ApplicationResultType Type { get; set; }
    public List<DiscordEmbed>? Embeds { get; set; }
    public string? Content { get; set; }
}

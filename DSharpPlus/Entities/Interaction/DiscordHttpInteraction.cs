using System.Net.Http;
using System.Threading.Tasks;

namespace DSharpPlus.Entities;

public class DiscordHttpInteraction : DiscordInteraction
{
    internal TaskCompletionSource taskCompletionSource = new();
    
    public override async Task CreateResponseAsync(DiscordInteractionResponseType type, DiscordInteractionResponseBuilder builder = null)
    {
        // TODO
        
        
    }
}

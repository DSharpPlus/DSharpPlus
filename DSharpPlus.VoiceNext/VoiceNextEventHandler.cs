using System.Threading.Tasks;

using DSharpPlus.EventArgs;

namespace DSharpPlus.VoiceNext;

internal sealed class VoiceNextEventHandler
    : IEventHandler<VoiceStateUpdatedEventArgs>,
    IEventHandler<VoiceServerUpdatedEventArgs>
{
    private readonly VoiceNextExtension extension;

    public VoiceNextEventHandler(VoiceNextExtension ext)
        => this.extension = ext;

    public async Task HandleEventAsync(DiscordClient sender, VoiceStateUpdatedEventArgs eventArgs) 
        => await this.extension.Client_VoiceStateUpdate(sender, eventArgs);

    public async Task HandleEventAsync(DiscordClient sender, VoiceServerUpdatedEventArgs eventArgs) 
        => await this.extension.Client_VoiceServerUpdateAsync(sender, eventArgs);
}

namespace DSharpPlus.Voice.Transport;

public interface IVoiceStateFactory
{
    public VoiceState Create(string userId, string serverId, string channelId, string token, string sessionId, string endpoint);
}

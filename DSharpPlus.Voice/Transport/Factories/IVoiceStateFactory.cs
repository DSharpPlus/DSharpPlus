namespace DSharpPlus.Voice.Transport.Factories;

/// <summary>
/// Factory used for making VoiceStates
/// </summary>
public interface IVoiceStateFactory
{
    /// <summary>
    /// Creates a VoiceStateFactory
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="serverId"></param>
    /// <param name="channelId"></param>
    /// <param name="token"></param>
    /// <param name="sessionId"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public VoiceState Create(string userId, string serverId, string channelId, string token, string sessionId, string endpoint);
}

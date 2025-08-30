namespace DSharpPlus.Voice.Transport.Factories;

/// <summary>
/// Factory used for making VoiceStates
/// </summary>
public interface IVoiceStateFactory
{
    /// <summary>
    /// Creates a VoiceStateFactory
    /// </summary>
    /// <returns>The created voicestate</returns>
    public VoiceState Create(string userId, string serverId, string channelId, string token, string sessionId, string endpoint);
}

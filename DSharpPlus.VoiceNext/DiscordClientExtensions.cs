using DSharpPlus.VoiceNext.Codec;

namespace DSharpPlus.VoiceNext
{
    public static class DiscordClientExtensions
    {
        private static VoiceNextClient ClientInstance { get; set; }

        /// <summary>
        /// Creates a new VoiceNext client with default settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextClient UseVoiceNext(this DiscordClient client) =>
            UseVoiceNext(client, new VoiceNextConfiguration { VoiceApplication = VoiceApplication.Music });

        /// <summary>
        /// Creates a new VoiceNext client with specified settings.
        /// </summary>
        /// <param name="client">Discord client to create VoiceNext instance for.</param>
        /// <param name="config">Configuration for the VoiceNext client.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextClient UseVoiceNext(this DiscordClient client, VoiceNextConfiguration config)
        {
            ClientInstance = new VoiceNextClient(config);
            client.AddModule(ClientInstance);
            return ClientInstance;
        }

        /// <summary>
        /// Gets the active instance of VoiceNext client for the DiscordClient.
        /// </summary>
        /// <param name="client">Discord client to get VoiceNext instance for.</param>
        /// <returns>VoiceNext client instance.</returns>
        public static VoiceNextClient GetVoiceNextClient(this DiscordClient client) =>
            ClientInstance;
    }
}

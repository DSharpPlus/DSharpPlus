namespace DSharpPlus.VoiceNext
{
    public static class DiscordClientExtensions
    {
        private static VoiceNextClient ClientInstance { get; set; }

        public static VoiceNextClient UseVoiceNext(this DiscordClient client) =>
            UseVoiceNext(client, new VoiceNextConfiguration { VoiceApplication = Codec.VoiceApplication.Music });

        public static VoiceNextClient UseVoiceNext(this DiscordClient client, VoiceNextConfiguration config)
        {
            ClientInstance = new VoiceNextClient(config);
            client.AddModule(ClientInstance);
            return ClientInstance;
        }

        public static VoiceNextClient GetVoiceNextClient(this DiscordClient client) =>
            ClientInstance;
    }
}

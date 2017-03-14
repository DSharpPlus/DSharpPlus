namespace DSharpPlus.VoiceNext
{
    public static class DiscordClientExtensions
    {
        private static VoiceNextClient ClientInstance { get; set; }

        public static VoiceNextClient UseVoiceNext(this DiscordClient client)
        {
            ClientInstance = new VoiceNextClient();
            client.AddModule(ClientInstance);
            return ClientInstance;
        }

        public static VoiceNextClient GetVoiceNextClient(this DiscordClient client) =>
            ClientInstance;
    }
}

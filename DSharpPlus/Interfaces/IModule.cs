namespace DSharpPlus
{
    public interface IModule
    {
        DiscordClient Client { get; }

        void Setup(DiscordClient client);
    }
}

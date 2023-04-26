using DSharpPlus.CH.Internals;

namespace DSharpPlus.CH;

public static class DiscordClientExtensionMethods
{
    public static void UseCH(this DiscordClient client, CHConfiguration config)
    {
        CommandController? controller = new(config, client);
    }
}

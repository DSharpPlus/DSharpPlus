using DSharpPlus.CH.Internals;

namespace DSharpPlus.CH;

public static class DiscordClientExtensionMethods
{
    public static CommandController UseCH(this DiscordClient client, CHBuilder builder)
    {
        CommandController controller = builder.Build(client);
        return controller;
    }
}

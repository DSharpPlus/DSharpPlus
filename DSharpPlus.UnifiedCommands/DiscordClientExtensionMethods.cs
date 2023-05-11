namespace DSharpPlus.UnifiedCommands;

public static class DiscordClientExtensionMethods
{
    public static CommandController UseCH(this DiscordClient client, UnifiedCommandsBuilder builder)
    {
        CommandController controller = builder.Build(client);
        return controller;
    }
}

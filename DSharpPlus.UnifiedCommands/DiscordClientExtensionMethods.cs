namespace DSharpPlus.UnifiedCommands;

public static class DiscordClientExtensionMethods
{
    /// <summary>
    ///  Constructs the CommandController from a builder.
    /// </summary>
    /// <param name="client">The discord client.</param>
    /// <param name="builder">The builder used for configuration.</param>
    /// <returns></returns>
    public static CommandController UseUnifiedCommands(this DiscordClient client, UnifiedCommandsBuilder builder)
    {
        CommandController controller = builder.Build(client);
        return controller;
    }
}

using System;
using System.Threading.Tasks;

namespace DSharpPlus.Commands
{
    public static class CommandExtension
    {
        public static CommandModule UseCommands(this DiscordClient client)
        {
            var module = new CommandModule();
            client.AddModule(module);
            return module;
        }

        public static CommandModule UseCommands(this DiscordClient client, CommandConfig config)
        {
            var module = new CommandModule(config);
            client.AddModule(module);
            return module;
        }

        public static CommandModule GetCommandModule(this DiscordClient client)
        {
            return client.GetModule<CommandModule>();
        }

        public static Command AddCommand(this DiscordClient client, string command, Func<CommandEventArgs, Task> Do) => client.GetCommandModule().AddCommand(command, Do);

        public static Command AddCommand(this DiscordClient client, string command, Action<CommandEventArgs> Do) => client.GetCommandModule().AddCommand(command, Do);
    }
}

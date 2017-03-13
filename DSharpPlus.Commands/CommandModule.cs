using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DSharpPlus.Commands
{
    public class CommandModule : IModule
    {
        internal static CommandModule instance;

        internal CommandConfig config { get; set; }

        public DiscordClient Client { get; set; }

        List<Command> _commands = new List<Command>();

        public CommandModule()
        {
            config = new CommandConfig();

            instance = this;
        }

        public CommandModule(CommandConfig config)
        {
            this.config = config;

            instance = this;
        }

        public void Setup(DiscordClient client)
        {
            Client = client;

            Client.MessageCreated += e =>
            {
                if (((e.Message.Author.ID != Client.Me.ID && !config.SelfBot) || (e.Message.Author.ID == Client.Me.ID && config.SelfBot))
                        && e.Message.Content.StartsWith(config.Prefix))
                {
                    string[] split = e.Message.Content.Split(new char[] { ' ' });
                    string cmdName = split[0].Substring(config.Prefix.Length);

                    foreach (Command command in _commands)
                    {
                        if (command.Name == cmdName)
                        {
                            command.Execute(new CommandEventArgs(e.Message, command, Client));
                        }
                    }
                }

                return Task.Delay(0);
            };
        }

        public Command AddCommand(string command, Func<CommandEventArgs, Task> Do)
        {
            Command cmd = new Command(command, Do);
            _commands.Add(cmd);

            Client.DebugLogger.LogMessage(LogLevel.Debug, "Command", $"Command added {command}", DateTime.Now);

            return cmd;
        }

        public Command AddCommand(string command, Action<CommandEventArgs> Do) => AddCommand(command, (x) => { Do(x); return Task.Delay(0); });
    }
}

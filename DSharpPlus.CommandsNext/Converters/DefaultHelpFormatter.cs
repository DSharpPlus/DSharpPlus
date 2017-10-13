using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters
{
    /// <summary>
    /// Default CommandsNext help formatter.
    /// </summary>
    public class DefaultHelpFormatter : IHelpFormatter
    {
        private DiscordEmbedBuilder _embed;
        private string _name, _desc;
        private bool _gexec;

        /// <summary>
        /// Creates a new default help formatter.
        /// </summary>
        public DefaultHelpFormatter()
        {
            _embed = new DiscordEmbedBuilder();
            _name = null;
            _desc = null;
            _gexec = false;
        }

        /// <summary>
        /// Sets the name of the current command.
        /// </summary>
        /// <param name="name">Name of the command for which the help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithCommandName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the description of the current command.
        /// </summary>
        /// <param name="description">Description of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithDescription(string description)
        {
            _desc = description;
            return this;
        }

        /// <summary>
        /// Sets aliases for the current command.
        /// </summary>
        /// <param name="aliases">Aliases of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            var enumerable = aliases as string[] ?? aliases.ToArray();
            if (enumerable.Any())
            {
                _embed.AddField("Aliases", string.Join(", ", enumerable.Select(Formatter.InlineCode)));
            }
            return this;
        }

        /// <summary>
        /// Sets the arguments the current command takes.
        /// </summary>
        /// <param name="arguments">Arguments that the command for which help is displayed takes.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            var commandArguments = arguments as CommandArgument[] ?? arguments.ToArray();
            if (commandArguments.Any())
            {
                var sb = new StringBuilder();

                foreach (var arg in commandArguments)
                {
                    if (arg.IsOptional || arg.IsCatchAll)
                    {
                        sb.Append("`[");
                    }
                    else
                    {
                        sb.Append("`<");
                    }

                    sb.Append(arg.Name);

                    if (arg.IsCatchAll)
                    {
                        sb.Append("...");
                    }

                    if (arg.IsOptional || arg.IsCatchAll)
                    {
                        sb.Append("]: ");
                    }
                    else
                    {
                        sb.Append(">: ");
                    }

                    sb.Append(arg.Type.ToUserFriendlyName()).Append("`: ");

                    sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : arg.Description);

                    if (arg.IsOptional)
                    {
                        sb.Append(" Default value: ").Append(arg.DefaultValue);
                    }

                    sb.AppendLine();
                }
                _embed.AddField("Arguments", sb.ToString());
            }
            return this;
        }

        /// <summary>
        /// When the current command is a group, this sets it as executable.
        /// </summary>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithGroupExecutable()
        {
            _gexec = true;
            return this;
        }

        /// <summary>
        /// Sets subcommands of the current command. This is also invoked for top-level command listing.
        /// </summary>
        /// <param name="subcommands">Subcommands of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var enumerable = subcommands as Command[] ?? subcommands.ToArray();
            if (enumerable.Any())
            {
                _embed.AddField(_name != null ? "Subcommands" : "Commands", string.Join(", ", enumerable.Select(xc => Formatter.InlineCode(xc.QualifiedName))));
            }
            return this;
        }

        /// <summary>
        /// Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        public CommandHelpMessage Build()
        {
            _embed.Title = "Help";
            _embed.Color = DiscordColor.Azure;

            var desc = "Listing all top-level commands and groups. Specify a command to see more information.";
            if (_name != null)
            {
                var sb = new StringBuilder();
                sb.Append(Formatter.InlineCode(_name))
                    .Append(": ")
                    .Append(string.IsNullOrWhiteSpace(_desc) ? "No description provided." : _desc);

                if (_gexec)
                {
                    sb.AppendLine().AppendLine().Append("This group can be executed as a standalone command.");
                }

                desc = sb.ToString();
            }
            _embed.Description = desc;

            return new CommandHelpMessage(embed: _embed);
        }
    }
}

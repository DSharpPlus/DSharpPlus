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
            this._embed = new DiscordEmbedBuilder();
            this._name = null;
            this._desc = null;
            this._gexec = false;
        }

        /// <summary>
        /// Sets the name of the current command.
        /// </summary>
        /// <param name="name">Name of the command for which the help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithCommandName(string name)
        {
            this._name = name;
            return this;
        }

        /// <summary>
        /// Sets the description of the current command.
        /// </summary>
        /// <param name="description">Description of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithDescription(string description)
        {
            this._desc = description;
            return this;
        }

        /// <summary>
        /// Sets aliases for the current command.
        /// </summary>
        /// <param name="aliases">Aliases of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            if (aliases.Any())
                this._embed.AddField("Aliases", string.Join(", ", aliases.Select(Formatter.InlineCode)), false);
            return this;
        }

        /// <summary>
        /// Sets the arguments the current command takes.
        /// </summary>
        /// <param name="arguments">Arguments that the command for which help is displayed takes.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            if (arguments.Any())
            {
                var sb = new StringBuilder();

                foreach (var arg in arguments)
                {
                    if (arg.IsOptional || arg.IsCatchAll)
                        sb.Append("`[");
                    else
                        sb.Append("`<");

                    sb.Append(arg.Name);

                    if (arg.IsCatchAll)
                        sb.Append("...");

                    if (arg.IsOptional || arg.IsCatchAll)
                        sb.Append("]: ");
                    else
                        sb.Append(">: ");

                    sb.Append(arg.Type.ToUserFriendlyName()).Append("`: ");

                    sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : arg.Description);

                    if (arg.IsOptional)
                        sb.Append(" Default value: ").Append(arg.DefaultValue);

                    sb.AppendLine();
                }
                this._embed.AddField("Arguments", sb.ToString(), false);
            }
            return this;
        }

        /// <summary>
        /// When the current command is a group, this sets it as executable.
        /// </summary>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithGroupExecutable()
        {
            this._gexec = true;
            return this;
        }

        /// <summary>
        /// Sets subcommands of the current command. This is also invoked for top-level command listing.
        /// </summary>
        /// <param name="subcommands">Subcommands of the command for which help is displayed.</param>
        /// <returns>Current formatter.</returns>
        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                this._embed.AddField(this._name != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(xc => Formatter.InlineCode(xc.QualifiedName))), false);
            return this;
        }

        /// <summary>
        /// Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        public CommandHelpMessage Build()
        {
            this._embed.Title = "Help";
            this._embed.Color = DiscordColor.Azure;

            var desc = "Listing all top-level commands and groups. Specify a command to see more information.";
            if (this._name != null)
            {
                var sb = new StringBuilder();
                sb.Append(Formatter.InlineCode(this._name))
                    .Append(": ")
                    .Append(string.IsNullOrWhiteSpace(this._desc) ? "No description provided." : this._desc);

                if (this._gexec)
                    sb.AppendLine().AppendLine().Append("This group can be executed as a standalone command.");

                desc = sb.ToString();
            }
            this._embed.Description = desc;

            return new CommandHelpMessage(embed: this._embed);
        }
    }
}

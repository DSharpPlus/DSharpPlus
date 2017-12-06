using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.Test
{
    public sealed class TestBotHelpFormatter : BaseHelpFormatter
    {
        private TestBotService Service { get; }

        private string _name = null, _desc = null, _args = null, _aliases = null, _subcs = null;
        private bool _gexec = false;

        public TestBotHelpFormatter(TestBotService dep, CommandsNextExtension cnext)
            : base(cnext)
        {
            this.Service = dep;
        }

        public override BaseHelpFormatter WithCommandName(string name)
        {
            this._name = name;
            return this;
        }

        public override BaseHelpFormatter WithDescription(string description)
        {
            this._desc = string.IsNullOrWhiteSpace(description) ? null : description;
            return this;
        }

        public override BaseHelpFormatter WithGroupExecutable()
        {
            this._gexec = true;
            return this;
        }

        public override BaseHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            if (aliases.Any())
                this._aliases = string.Join(", ", aliases);
            return this;
        }

        public override BaseHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            if (arguments.Any())
                this._args = string.Join(", ", arguments.Select(xa => $"{xa.Name}: {this.CommandsNext.TypeToUserFriendlyName(xa.Type)}"));
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
            {
                var ml = subcommands.Max(xc => xc.Name.Length);
                var sb = new StringBuilder();
                foreach (var xc in subcommands)
                    sb.Append(xc.Name.PadRight(ml, ' '))
                        .Append("  ")
                        .Append(string.IsNullOrWhiteSpace(xc.Description) ? "No description." : xc.Description).Append("\n");
                this._subcs = sb.ToString();
            }
            return this;
        }

        public override CommandHelpMessage Build()
        {
            var sb = new StringBuilder();
            sb.Append("```less\n");
            if (this._name == null)
            {
                if (this._subcs != null)
                    sb.Append("Displaying all available commands.\n\n");
                else
                    sb.Append("No commands are available.");
            }
            else
            {
                sb.Append(this._name).Append("\n\n");

                if (this._gexec)
                    sb.Append("This group can be executed as a standalone command.\n\n");

                if (this._desc != null)
                    sb.Append("Description: ").Append(this._desc).Append("\n");

                if (this._args != null)
                    sb.Append("Arguments:   ").Append(this._args).Append("\n");

                if (this._aliases != null)
                    sb.Append("Aliases:     ").Append(this._aliases).Append("\n");

                if (this._subcs != null)
                    sb.Append("Subcommands:\n\n");
            }

            if (this._subcs != null)
                sb.Append(_subcs);
            else
                sb.Append("\n");

            sb.Append("```");
            return new CommandHelpMessage(sb.ToString());
        }
    }
}

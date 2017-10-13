using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.Test
{
    public sealed class TestBotHelpFormatter : IHelpFormatter
    {
        private string _name, _desc, _args, _aliases, _subcs;
        private bool _gexec;

        public IHelpFormatter WithCommandName(string name)
        {
            _name = name;
            return this;
        }

        public IHelpFormatter WithDescription(string description)
        {
            _desc = string.IsNullOrWhiteSpace(description) ? null : description;
            return this;
        }

        public IHelpFormatter WithGroupExecutable()
        {
            _gexec = true;
            return this;
        }

        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            var enumerable = aliases as string[] ?? aliases.ToArray();
            if (enumerable.Any())
            {
                _aliases = string.Join(", ", enumerable);
            }
            return this;
        }

        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            var commandArguments = arguments as CommandArgument[] ?? arguments.ToArray();
            if (commandArguments.Any())
            {
                _args = string.Join(", ", commandArguments.Select(xa => $"{xa.Name}: {xa.Type.ToUserFriendlyName()}"));
            }
            return this;
        }

        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var enumerable = subcommands as Command[] ?? subcommands.ToArray();
            if (enumerable.Any())
            {
                var ml = enumerable.Max(xc => xc.Name.Length);
                var sb = new StringBuilder();
                foreach (var xc in enumerable)
                {
                    sb.Append(xc.Name.PadRight(ml, ' '))
                      .Append("  ")
                      .Append(string.IsNullOrWhiteSpace(xc.Description) ? "No description." : xc.Description).Append("\n");
                }

                _subcs = sb.ToString();
            }
            return this;
        }

        public CommandHelpMessage Build()
        {
            var sb = new StringBuilder();
            sb.Append("```less\n");
            if (_name == null)
            {
                if (_subcs != null)
                {
                    sb.Append("Displaying all available commands.\n\n");
                }
                else
                {
                    sb.Append("No commands are available.");
                }
            }
            else
            {
                sb.Append(_name).Append("\n\n");

                if (_gexec)
                {
                    sb.Append("This group can be executed as a standalone command.\n\n");
                }

                if (_desc != null)
                {
                    sb.Append("Description: ").Append(_desc).Append("\n");
                }

                if (_args != null)
                {
                    sb.Append("Arguments:   ").Append(_args).Append("\n");
                }

                if (_aliases != null)
                {
                    sb.Append("Aliases:     ").Append(_aliases).Append("\n");
                }

                if (_subcs != null)
                {
                    sb.Append("Subcommands:\n\n");
                }
            }

            if (_subcs != null)
            {
                sb.Append(_subcs);
            }
            else
            {
                sb.Append("\n");
            }

            sb.Append("```");
            return new CommandHelpMessage(sb.ToString());
        }
    }
}

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
        private StringBuilder Content { get; }

        public TestBotHelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this.Content = new StringBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Content.Append(command.Description ?? "No description provided.").Append("\n\n");

            if (command.Aliases?.Any() == true)
                this.Content.Append("Aliases: ").Append(string.Join(", ", command.Aliases)).Append("\n\n");

            if (command.Overloads?.Any() == true)
            {
                var sb = new StringBuilder();

                foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    sb.Append(command.QualifiedName);

                    foreach (var arg in ovl.Arguments)
                        sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                    sb.Append('\n');

                    foreach (var arg in ovl.Arguments)
                        sb.Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append("): ").Append(arg.Description ?? "No description provided.").Append('\n');

                    sb.Append('\n');
                }

                this.Content.Append("Arguments:\n").Append(sb.ToString());
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (this.Content.Length == 0)
                this.Content.Append("Displaying all available commands.\n\n");
            else
                this.Content.Append("Subcommands:\n");

            if (subcommands?.Any() == true)
            {
                var ml = subcommands.Max(xc => xc.Name.Length);
                var sb = new StringBuilder();
                foreach (var xc in subcommands)
                    sb.Append(xc.Name.PadRight(ml, ' '))
                        .Append("  ")
                        .Append(string.IsNullOrWhiteSpace(xc.Description) ? "" : xc.Description).Append("\n");
                this.Content.Append(sb.ToString());
            }

            return this;
        }

        public override CommandHelpMessage Build()
            => new CommandHelpMessage($"```less\n{this.Content.ToString().Trim()}\n```");
    }
}

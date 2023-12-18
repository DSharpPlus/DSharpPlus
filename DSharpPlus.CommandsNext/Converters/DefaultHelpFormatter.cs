using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace DSharpPlus.CommandsNext.Converters;

/// <summary>
/// Default CommandsNext help formatter.
/// </summary>
public class DefaultHelpFormatter : BaseHelpFormatter
{
    public DiscordEmbedBuilder EmbedBuilder { get; }
    private Command? Command { get; set; }

    /// <summary>
    /// Creates a new default help formatter.
    /// </summary>
    /// <param name="ctx">Context in which this formatter is being invoked.</param>
    public DefaultHelpFormatter(CommandContext ctx)
        : base(ctx)
    {
        this.EmbedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Help")
            .WithColor(0x007FFF);
    }

    /// <summary>
    /// Sets the command this help message will be for.
    /// </summary>
    /// <param name="command">Command for which the help message is being produced.</param>
    /// <returns>This help formatter.</returns>
    public override BaseHelpFormatter WithCommand(Command command)
    {
        this.Command = command;

        this.EmbedBuilder.WithDescription($"{Formatter.InlineCode(command.Name)}: {command.Description ?? "No description provided."}");

        if (command is CommandGroup cgroup && cgroup.IsExecutableWithoutSubcommands)
        {
            this.EmbedBuilder.WithDescription($"{this.EmbedBuilder.Description}\n\nThis group can be executed as a standalone command.");
        }

        if (command.Aliases.Count > 0)
        {
            this.EmbedBuilder.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)), false);
        }

        if (command.Overloads.Count > 0)
        {
            StringBuilder sb = new StringBuilder();

            foreach (CommandOverload? ovl in command.Overloads.OrderByDescending(x => x.Priority))
            {
                sb.Append('`').Append(command.QualifiedName);

                foreach (CommandArgument arg in ovl.Arguments)
                {
                    sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');
                }

                sb.Append("`\n");

                foreach (CommandArgument arg in ovl.Arguments)
                {
                    sb.Append('`').Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "No description provided.").Append('\n');
                }

                sb.Append('\n');
            }

            this.EmbedBuilder.AddField("Arguments", sb.ToString().Trim(), false);
        }

        return this;
    }

    /// <summary>
    /// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
    /// </summary>
    /// <param name="subcommands">Subcommands for this command group.</param>
    /// <returns>This help formatter.</returns>
    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    {

        IOrderedEnumerable<IGrouping<string?, Command>> categories = subcommands.GroupBy(xm => xm.Category).OrderBy(xm => xm.Key == null).ThenBy(xm => xm.Key);

        // no known categories, proceed without categorization
        if (categories.Count() == 1 && categories.Single().Key == null)
        {
            this.EmbedBuilder.AddField(this.Command is not null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(x => Formatter.InlineCode(x.Name))), false);

            return this;
        }

        foreach (IGrouping<string?, Command>? category in categories)
        {
            this.EmbedBuilder.AddField(category.Key ?? "Uncategorized commands", string.Join(", ", category.Select(xm => Formatter.InlineCode(xm.Name))), false);
        }

        return this;
    }

    /// <summary>
    /// Construct the help message.
    /// </summary>
    /// <returns>Data for the help message.</returns>
    public override CommandHelpMessage Build()
    {
        if (this.Command is null)
        {
            this.EmbedBuilder.WithDescription("Listing all top-level commands and groups. Specify a command to see more information.");
        }

        return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
    }
}

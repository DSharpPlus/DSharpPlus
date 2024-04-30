using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext;

/// <summary>
/// Represents a command.
/// </summary>
public class Command
{
    /// <summary>
    /// Gets this command's name.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the category this command belongs to.
    /// </summary>
    public string? Category { get; internal set; } = null;

    /// <summary>
    /// Gets this command's qualified name (i.e. one that includes all module names).
    /// </summary>
    public string QualifiedName => Parent is not null ? string.Concat(Parent.QualifiedName, " ", Name) : Name;

    /// <summary>
    /// Gets this command's aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; internal set; } = Array.Empty<string>();

    /// <summary>
    /// Gets this command's parent module, if any.
    /// </summary>
    public CommandGroup? Parent { get; internal set; }

    /// <summary>
    /// Gets this command's description.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// Gets whether this command is hidden.
    /// </summary>
    public bool IsHidden { get; internal set; }

    /// <summary>
    /// Gets a collection of pre-execution checks for this command.
    /// </summary>
    public IReadOnlyList<CheckBaseAttribute> ExecutionChecks { get; internal set; } = Array.Empty<CheckBaseAttribute>();

    /// <summary>
    /// Gets a collection of this command's overloads.
    /// </summary>
    public IReadOnlyList<CommandOverload> Overloads { get; internal set; } = Array.Empty<CommandOverload>();

    /// <summary>
    /// Gets the module in which this command is defined.
    /// </summary>
    public ICommandModule? Module { get; internal set; }

    /// <summary>
    /// Gets the custom attributes defined on this command.
    /// </summary>
    public IReadOnlyList<Attribute> CustomAttributes { get; internal set; } = Array.Empty<Attribute>();

    internal Command() { }

    /// <summary>
    /// Executes this command with specified context.
    /// </summary>
    /// <param name="ctx">Context to execute the command in.</param>
    /// <returns>Command's execution results.</returns>
    public virtual async Task<CommandResult> ExecuteAsync(CommandContext ctx)
    {
        try
        {
            foreach (CommandOverload? overload in Overloads.OrderByDescending(x => x.Priority))
            {
                ctx.Overload = overload;

                // Attempt to match the arguments to the overload
                ArgumentBindingResult args = await CommandsNextUtilities.BindArgumentsAsync(ctx, ctx.Config.IgnoreExtraArguments);
                if (!args.IsSuccessful)
                {
                    continue;
                }

                ctx.RawArguments = args.Raw;

                // From... what I can gather, this seems to be support for executing commands that don't inherit from BaseCommandModule.
                // But, that can never be the case since all Commands must inherit from BaseCommandModule.
                // Regardless, I'm not removing this legacy code in case if it's actually used and I'm just not seeing it.
                BaseCommandModule? commandModule = Module?.GetInstance(ctx.Services);
                if (commandModule is not null)
                {
                    await commandModule.BeforeExecutionAsync(ctx);
                }

                args.Converted[0] = overload._invocationTarget ?? commandModule;
                await (Task)overload._callable.DynamicInvoke(args.Converted)!;

                if (commandModule is not null)
                {
                    await commandModule.AfterExecutionAsync(ctx);
                }

                return new CommandResult
                {
                    IsSuccessful = true,
                    Context = ctx
                };
            }

            throw new ArgumentException("Could not find a suitable overload for the command.");
        }
        catch (Exception error)
        {
            if (error is TargetInvocationException targetInvocationError)
            {
                error = ExceptionDispatchInfo.Capture(targetInvocationError.InnerException!).SourceException;
            }

            return new CommandResult
            {
                IsSuccessful = false,
                Exception = error,
                Context = ctx
            };
        }
    }

    /// <summary>
    /// Runs pre-execution checks for this command and returns any that fail for given context.
    /// </summary>
    /// <param name="ctx">Context in which the command is executed.</param>
    /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
    /// <returns>Pre-execution checks that fail for given context.</returns>
    public async Task<IEnumerable<CheckBaseAttribute>> RunChecksAsync(CommandContext ctx, bool help)
    {
        List<CheckBaseAttribute> fchecks = new List<CheckBaseAttribute>();
        if (ExecutionChecks.Any())
        {
            foreach (CheckBaseAttribute ec in ExecutionChecks)
            {
                if (!await ec.ExecuteCheckAsync(ctx, help))
                {
                    fchecks.Add(ec);
                }
            }
        }

        return fchecks;
    }

    /// <summary>
    /// Checks whether this command is equal to another one.
    /// </summary>
    /// <param name="cmd1">Command to compare to.</param>
    /// <param name="cmd2">Command to compare.</param>
    /// <returns>Whether the two commands are equal.</returns>
    public static bool operator ==(Command? cmd1, Command? cmd2)
    {
        object? o1 = cmd1 as object;
        object? o2 = cmd2 as object;
        return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || cmd1!.QualifiedName == cmd2!.QualifiedName);
    }

    /// <summary>
    /// Checks whether this command is not equal to another one.
    /// </summary>
    /// <param name="cmd1">Command to compare to.</param>
    /// <param name="cmd2">Command to compare.</param>
    /// <returns>Whether the two commands are not equal.</returns>
    public static bool operator !=(Command? cmd1, Command? cmd2) => !(cmd1 == cmd2);

    /// <summary>
    /// Checks whether this command equals another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether this command is equal to another object.</returns>
    public override bool Equals(object? obj)
    {
        object? o2 = this as object;
        return (obj != null || o2 == null) && (obj == null || o2 != null) && ((obj == null && o2 == null) || (obj is Command cmd && cmd.QualifiedName == QualifiedName));
    }

    /// <summary>
    /// Gets this command's hash code.
    /// </summary>
    /// <returns>This command's hash code.</returns>
    public override int GetHashCode() => QualifiedName.GetHashCode();

    /// <summary>
    /// Returns a string representation of this command.
    /// </summary>
    /// <returns>String representation of this command.</returns>
    public override string ToString() => this is CommandGroup g
            ? $"Command Group: {QualifiedName}, {g.Children.Count} top-level children"
            : $"Command: {QualifiedName}";
}

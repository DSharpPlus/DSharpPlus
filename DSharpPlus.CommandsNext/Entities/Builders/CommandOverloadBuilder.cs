using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;

namespace DSharpPlus.CommandsNext.Builders;

/// <summary>
/// Represents an interface to build a command overload.
/// </summary>
public sealed class CommandOverloadBuilder
{
    /// <summary>
    /// Gets a value that uniquely identifies an overload.
    /// </summary>
    internal string argumentSet { get; }

    /// <summary>
    /// Gets the collection of arguments this overload takes.
    /// </summary>
    public IReadOnlyList<CommandArgument> Arguments { get; } = Array.Empty<CommandArgument>();

    /// <summary>
    /// Gets this overload's priority when picking a suitable one for execution.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets the overload's callable delegate.
    /// </summary>
    public Delegate Callable { get; set; }

    private object? invocationTarget { get; }

    /// <summary>
    /// Creates a new command overload builder from specified method.
    /// </summary>
    /// <param name="method">Method to use for this overload.</param>
    public CommandOverloadBuilder(MethodInfo method) : this(method, null) { }

    /// <summary>
    /// Creates a new command overload builder from specified delegate.
    /// </summary>
    /// <param name="method">Delegate to use for this overload.</param>
    public CommandOverloadBuilder(Delegate method) : this(method.GetMethodInfo(), method.Target) { }

    private CommandOverloadBuilder(MethodInfo method, object? target)
    {
        if (!method.IsCommandCandidate(out ParameterInfo[]? prms))
        {
            throw new ArgumentException("Specified method is not suitable for a command.", nameof(method));
        }

        this.invocationTarget = target;

        // create the argument array
        ParameterExpression[] ea = new ParameterExpression[prms.Length + 1];
        ParameterExpression iep = Expression.Parameter(target?.GetType() ?? method.DeclaringType, "instance");
        ea[0] = iep;
        ea[1] = Expression.Parameter(typeof(CommandContext), "ctx");

        PriorityAttribute? pri = method.GetCustomAttribute<PriorityAttribute>();
        if (pri != null)
        {
            this.Priority = pri.Priority;
        }

        int i = 2;
        List<CommandArgument> args = new(prms.Length - 1);
        StringBuilder setb = new();
        foreach (ParameterInfo? arg in prms.Skip(1))
        {
            setb.Append(arg.ParameterType).Append(';');
            CommandArgument ca = new()
            {
                Name = arg.Name,
                Type = arg.ParameterType,
                IsOptional = arg.IsOptional,
                DefaultValue = arg.IsOptional ? arg.DefaultValue : null
            };

            List<Attribute> attrsCustom = [];
            IEnumerable<Attribute> attrs = arg.GetCustomAttributes();
            bool isParams = false;
            foreach (Attribute xa in attrs)
            {
                switch (xa)
                {
                    case DescriptionAttribute d:
                        ca.Description = d.Description;
                        break;

                    case RemainingTextAttribute:
                        ca.IsCatchAll = true;
                        break;

                    case ParamArrayAttribute:
                        ca.IsCatchAll = true;
                        ca.Type = arg.ParameterType.GetElementType();
                        ca.isArray = true;
                        isParams = true;
                        break;

                    default:
                        attrsCustom.Add(xa);
                        break;
                }
            }

            if (i > 2 && !ca.IsOptional && !ca.IsCatchAll && args[i - 3].IsOptional)
            {
                throw new InvalidOverloadException("Non-optional argument cannot appear after an optional one", method, arg);
            }

            if (arg.ParameterType.IsArray && !isParams)
            {
                throw new InvalidOverloadException("Cannot use array arguments without params modifier.", method, arg);
            }

            ca.CustomAttributes = new ReadOnlyCollection<Attribute>(attrsCustom);
            args.Add(ca);
            ea[i++] = Expression.Parameter(arg.ParameterType, arg.Name);
        }

        //var ec = Expression.Call(iev, method, ea.Skip(2));
        MethodCallExpression ec = Expression.Call(iep, method, ea.Skip(1));
        LambdaExpression el = Expression.Lambda(ec, ea);

        this.argumentSet = setb.ToString();
        this.Arguments = new ReadOnlyCollection<CommandArgument>(args);
        this.Callable = el.Compile();
    }

    /// <summary>
    /// Sets the priority for this command overload.
    /// </summary>
    /// <param name="priority">Priority for this command overload.</param>
    /// <returns>This builder.</returns>
    public CommandOverloadBuilder WithPriority(int priority)
    {
        this.Priority = priority;
        return this;
    }

    internal CommandOverload Build()
    {
        CommandOverload ovl = new()
        {
            Arguments = this.Arguments,
            Priority = this.Priority,
            callable = this.Callable,
            invocationTarget = this.invocationTarget
        };

        return ovl;
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.CommandsNext.Builders
{
    /// <summary>
    /// Represents an interface to build a command overload.
    /// </summary>
    public sealed class CommandOverloadBuilder
    {
        /// <summary>
        /// Gets a value that uniquely identifies an overload.
        /// </summary>
        internal string ArgumentSet { get; }

        /// <summary>
        /// Gets the collection of arguments this overload takes.
        /// </summary>
        public IReadOnlyList<CommandArgument> Arguments { get; }

        /// <summary>
        /// Gets this overload's priority when picking a suitable one for execution.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets the overload's callable delegate.
        /// </summary>
        public Delegate Callable { get; set; }

        /// <summary>
        /// Creates a new command overload builder from specified method.
        /// </summary>
        /// <param name="method">Method to use for this overload.</param>
        /// <param name="moduleInstance">Instance of the module this command will use when called.</param>
        public CommandOverloadBuilder(MethodInfo method, object moduleInstance)
        {
            if (!method.IsCommandCandidate(out var prms))
                throw new MissingMethodException("Specified method is not suitable for a command.");

            var ei = Expression.Constant(moduleInstance);
            var ea = new ParameterExpression[prms.Length];
            ea[0] = Expression.Parameter(typeof(CommandContext), "ctx");

            var pri = method.GetCustomAttribute<PriorityAttribute>();
            if (pri != null)
                this.Priority = pri.Priority;

            var i = 1;
            var args = new List<CommandArgument>(prms.Length - 1);
            var setb = new StringBuilder();
            foreach (var arg in prms.Skip(1))
            {
                setb.Append(arg.ParameterType).Append(";");
                var ca = new CommandArgument
                {
                    Name = arg.Name,
                    Type = arg.ParameterType,
                    IsOptional = arg.IsOptional,
                    DefaultValue = arg.IsOptional ? arg.DefaultValue : null
                };

                var attrs = arg.GetCustomAttributes();
                foreach (var xa in attrs)
                {
                    switch (xa)
                    {
                        case DescriptionAttribute d:
                            ca.Description = d.Description;
                            break;

                        case RemainingTextAttribute r:
                            ca.IsCatchAll = true;
                            break;

                        case ParamArrayAttribute p:
                            ca.IsCatchAll = true;
                            ca.Type = arg.ParameterType.GetElementType();
                            ca._isArray = true;
                            break;
                    }
                }

                if (i > 1 && !ca.IsOptional && !ca.IsCatchAll && args[i - 2].IsOptional)
                    throw new InvalidOperationException("Non-optional argument cannot appear after an optional one");

                args.Add(ca);
                ea[i++] = Expression.Parameter(arg.ParameterType, arg.Name);
            }

            var ec = Expression.Call(ei, method, ea);
            var el = Expression.Lambda(ec, ea);

            this.ArgumentSet = setb.ToString();
            this.Arguments = new ReadOnlyCollection<CommandArgument>(args);
            this.Callable = el.Compile();
        }

        internal CommandOverload Build()
        {
            var ovl = new CommandOverload()
            {
                Arguments = this.Arguments,
                Priority = this.Priority,
                Callable = this.Callable
            };

            return ovl;
        }
    }
}

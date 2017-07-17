using System;
using System.Linq;

namespace DSharpPlus.CommandsNext.Attributes
{
    /// <summary>
    /// Marks this class as a command group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class GroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this group.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Whether or not this group can be invoked without subcommand. If this is set to true, the group needs to have a ExecuteGroup method.
        /// </summary>
        public bool CanInvokeWithoutSubcommand { get; set; } = false;

        /// <summary>
        /// Marks this class as a command group with specified name.
        /// </summary>
        /// <param name="name">Name of this group.</param>
        public GroupAttribute(string name)
        {
            if (name.Any(xc => char.IsWhiteSpace(xc)))
                throw new ArgumentException("Group names cannot contain whitespace characters.", nameof(name));

            this.Name = name;
        }
    }
}

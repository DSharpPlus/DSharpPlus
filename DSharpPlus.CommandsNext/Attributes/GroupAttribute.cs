﻿using System;
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
        public string Name { get; }

        /// <summary>
        /// Marks this class as a command group, using the class' name as group name.
        /// </summary>
        public GroupAttribute()
        {
            this.Name = null;
        }

        /// <summary>
        /// Marks this class as a command group with specified name.
        /// </summary>
        /// <param name="name">Name of this group.</param>
        public GroupAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "Group names cannot be null, empty, or all-whitespace.");

            if (name.Any(xc => char.IsWhiteSpace(xc)))
                throw new ArgumentException("Group names cannot contain whitespace characters.", nameof(name));

            this.Name = name;
        }
    }
}

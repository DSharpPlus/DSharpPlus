using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Marks this class a slash command group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SlashCommandGroupAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this slash command group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this slash command group.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets whether this command is enabled on default.
        /// </summary>
        public bool DefaultPermission { get; }

        /// <summary>
        /// Marks this class as a slash command group.
        /// </summary>
        /// <param name="name">Sets the name of this command group.</param>
        /// <param name="description">Sets the description of this command group.</param>
        /// <param name="defaultPermission">Sets whether this command group is enabled on default.</param>
        public SlashCommandGroupAttribute(string name, string description, bool defaultPermission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = defaultPermission;
        }
    }
}

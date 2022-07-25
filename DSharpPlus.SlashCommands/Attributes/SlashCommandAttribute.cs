using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Marks this method as a slash command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of this command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of this command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets whether this command is enabled by default.
        /// </summary>
        public bool DefaultPermission { get; }

        /// <summary>
        /// Marks this method as a slash command.
        /// </summary>
        /// <param name="name">Sets the name of this slash command.</param>
        /// <param name="description">Sets the description of this slash command.</param>
        /// <param name="defaultPermission">Sets whether the command should be enabled by default.</param>
        public SlashCommandAttribute(string name, string description, bool defaultPermission = true)
        {
            this.Name = name.ToLower();
            this.Description = description;
            this.DefaultPermission = defaultPermission;
        }
    }
}

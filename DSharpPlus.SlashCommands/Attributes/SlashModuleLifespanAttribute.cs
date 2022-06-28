using System;

namespace DSharpPlus.SlashCommands
{
    /// <summary>
    /// Defines this slash command module's lifespan. Module lifespans are transient by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SlashModuleLifespanAttribute : Attribute
    {
        /// <summary>
        /// Gets the lifespan.
        /// </summary>
        public SlashModuleLifespan Lifespan { get; }

        /// <summary>
        /// Defines this slash command module's lifespan.
        /// </summary>
        /// <param name="lifespan">The lifespan of the module. Module lifespans are transient by default.</param>
        public SlashModuleLifespanAttribute(SlashModuleLifespan lifespan)
        {
            this.Lifespan = lifespan;
        }
    }

    /// <summary>
    /// Represents a slash command module lifespan.
    /// </summary>
    public enum SlashModuleLifespan
    {
        /// <summary>
        /// Whether this module should be initiated every time a command is run, with dependencies injected from a scope.
        /// </summary>
        Scoped,

        /// <summary>
        /// Whether this module should be initiated every time a command is run.
        /// </summary>
        Transient,

        /// <summary>
        /// Whether this module should be initiated at startup.
        /// </summary>
        Singleton
    }
}

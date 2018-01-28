using System;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Builders
{
    /// <summary>
    /// Represents an interface to build a command module.
    /// </summary>
    public sealed class CommandModuleBuilder
    {
        /// <summary>
        /// Gets the type this build will construct a module out of.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the lifespan for the built module.
        /// </summary>
        public ModuleLifespan Lifespan { get; private set; }

        /// <summary>
        /// Creates a new command module builder.
        /// </summary>
        public CommandModuleBuilder()
        { }

        /// <summary>
        /// Sets the type this builder will construct a module out of.
        /// </summary>
        /// <param name="t">Type to build a module out of. It has to derive from <see cref="BaseCommandModule"/>.</param>
        /// <returns>This builder.</returns>
        public CommandModuleBuilder WithType(Type t)
        {
            if (!t.IsModuleCandidateType())
                throw new ArgumentException("Specified type is not a valid module type.", nameof(t));

            this.Type = t;
            return this;
        }

        /// <summary>
        /// Lifespan to give this module.
        /// </summary>
        /// <param name="lifespan">Lifespan for this module.</param>
        /// <returns>This builder.</returns>
        public CommandModuleBuilder WithLifespan(ModuleLifespan lifespan)
        {
            this.Lifespan = lifespan;
            return this;
        }

        internal ICommandModule Build(IServiceProvider services)
        {
            switch (this.Lifespan)
            {
                case ModuleLifespan.Singleton:
                    return new SingletonCommandModule(this.Type, services);

                case ModuleLifespan.Transient:
                    return new TransientCommandModule(this.Type);
            }

            throw new NotSupportedException("Module lifespans other than transient and singleton are not supported.");
        }
    }
}

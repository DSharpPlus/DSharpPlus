using System;

namespace DSharpPlus.CommandsNext.Entities
{
    /// <summary>
    /// Represents a base interface for all types of command modules.
    /// </summary>
    public interface ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        Type ModuleType { get; }

        /// <summary>
        /// Returns an instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>A created instance of this module.</returns>
        BaseCommandModule GetInstance(IServiceProvider services);
    }

    /// <summary>
    /// Represents a transient command module. This type of module is reinstantiated on every command call.
    /// </summary>
    public class TransientCommandModule : ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Creates a new transient module.
        /// </summary>
        /// <param name="t">Type of the module to create.</param>
        internal TransientCommandModule(Type t)
        {
            this.ModuleType = t;
        }

        /// <summary>
        /// Creates a new instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>Created module.</returns>
        public BaseCommandModule GetInstance(IServiceProvider services)
            => this.ModuleType.CreateInstance(services) as BaseCommandModule;
    }

    /// <summary>
    /// Represents a singleton command module. This type of module is instantiated only when created.
    /// </summary>
    public class SingletonCommandModule : ICommandModule
    {
        /// <summary>
        /// Gets the type of this module.
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Gets this module's instance.
        /// </summary>
        public BaseCommandModule Instance { get; }

        /// <summary>
        /// Creates a new singleton module, and instantiates it.
        /// </summary>
        /// <param name="t">Type of the module to create.</param>
        /// <param name="services">Services to instantiate the module with.</param>
        internal SingletonCommandModule(Type t, IServiceProvider services)
        {
            this.ModuleType = t;
            this.Instance = t.CreateInstance(services) as BaseCommandModule;
        }

        /// <summary>
        /// Returns the instance of this module.
        /// </summary>
        /// <param name="services">Services to instantiate the module with.</param>
        /// <returns>This module's instance.</returns>
        public BaseCommandModule GetInstance(IServiceProvider services)
            => this.Instance;
    }
}

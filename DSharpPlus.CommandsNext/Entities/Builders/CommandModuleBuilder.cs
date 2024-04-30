using System;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;

namespace DSharpPlus.CommandsNext.Builders;

/// <summary>
/// Represents an interface to build a command module.
/// </summary>
public sealed class CommandModuleBuilder
{
    /// <summary>
    /// Gets the type this build will construct a module out of.
    /// </summary>
    public Type Type { get; private set; } = null!;

    /// <summary>
    /// Gets the lifespan for the built module.
    /// </summary>
    public ModuleLifespan Lifespan { get; private set; }

    /// <summary>
    /// Creates a new command module builder.
    /// </summary>
    public CommandModuleBuilder() { }

    /// <summary>
    /// Sets the type this builder will construct a module out of.
    /// </summary>
    /// <param name="t">Type to build a module out of. It has to derive from <see cref="BaseCommandModule"/>.</param>
    /// <returns>This builder.</returns>
    public CommandModuleBuilder WithType(Type t)
    {
        if (!t.IsModuleCandidateType())
        {
            throw new ArgumentException("Specified type is not a valid module type.", nameof(t));
        }

        Type = t;
        return this;
    }

    /// <summary>
    /// Lifespan to give this module.
    /// </summary>
    /// <param name="lifespan">Lifespan for this module.</param>
    /// <returns>This builder.</returns>
    public CommandModuleBuilder WithLifespan(ModuleLifespan lifespan)
    {
        Lifespan = lifespan;
        return this;
    }

    internal ICommandModule Build(IServiceProvider services) => Type is null
            ? throw new InvalidOperationException($"A command module cannot be built without a module type, please use the {nameof(this.WithType)} method to set a type.")
            : Lifespan switch
            {
                ModuleLifespan.Singleton => new SingletonCommandModule(Type, services),
                ModuleLifespan.Transient => new TransientCommandModule(Type),
                _ => throw new NotSupportedException("Module lifespans other than transient and singleton are not supported."),
            };
}

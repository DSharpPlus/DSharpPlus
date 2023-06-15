using System.Reflection;
using DSharpPlus.UnifiedCommands.Application.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands;

// TODO: Add support to manually add modules.
public class UnifiedCommandsBuilder
{
    private readonly List<Assembly> _assemblies = new();
    private bool _isAlreadyBuilt = false;
    private readonly List<string> _prefixes = new();
    private ulong[]? _guildIds = null;
    private bool _allowSlashCommands = true;
    private readonly List<Type> _interactionConditions = new();

    /// <summary>
    /// The service collection used for building the service provider.
    /// </summary>
    public IServiceCollection Services { get; internal set; } = new ServiceCollection();

    /// <summary>
    /// Add the assembly used for reflection.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>Returns the builder.</returns>
    public UnifiedCommandsBuilder AddAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    /// <summary>
    /// Adds your prefixes.
    /// </summary>
    /// <param name="prefixes">The prefixes your bot uses.</param>
    /// <returns>Returns the builder.</returns>
    public UnifiedCommandsBuilder AddPrefix(params string[] prefixes)
    {
        _prefixes.AddRange(prefixes);
        return this;
    }

    /// <summary>
    /// Adds guild ids for application command registration.
    /// </summary>
    /// <param name="guildIds">The guild ids.</param>
    /// <returns>Returns the builder.</returns>
    public UnifiedCommandsBuilder AddGuilds(params ulong[] guildIds)
    {
        _guildIds = guildIds;
        return this;
    }

    /// <summary>
    /// Decides if slash commands should be used or not.
    /// </summary>
    /// <param name="bool">The bool for it.</param>
    /// <returns></returns>
    public UnifiedCommandsBuilder AllowSlashCommands(bool @bool)
    {
        _allowSlashCommands = @bool;
        return this;
    }

    public UnifiedCommandsBuilder AddInteractionCondition<T>(ServiceLifetime lifetime) where T : IApplicationCondition
    {
        Type type = typeof(T);

        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                Services.AddScoped(type);
                break;
            case ServiceLifetime.Singleton:
                Services.AddSingleton(type);
                break;
            case ServiceLifetime.Transient:
                Services.AddTransient(type);
                break;
        }

        _interactionConditions.Add(type);
        return this;
    }

    public UnifiedCommandsBuilder AddInteractionCondition(Type type, ServiceLifetime lifetime)
    {
        if (!type.IsSubclassOf(typeof(IApplicationCondition)))
        {
            throw new Exception("You cannot use that as a type"); // TODO: Make this into it's own exception.
        }

        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                Services.AddScoped(type);
                break;
            case ServiceLifetime.Singleton:
                Services.AddSingleton(type);
                break;
            case ServiceLifetime.Transient:
                Services.AddTransient(type);
                break;
        }

        _interactionConditions.Add(type);
        return this;
    }

    /// <summary>
    /// Internal usage for building for the <see cref="CommandController">CommandController</see>
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal CommandController Build(DiscordClient client)
    {
        _isAlreadyBuilt = !_isAlreadyBuilt ? true : throw new Exception("The builder has already built an object.");

        if (!Services.Any(s => s.ServiceType == typeof(IErrorHandler)))
        {
            client.Logger.LogTrace("Didn't find a error handler, using the default error handler");
            Services.AddSingleton<IErrorHandler, Internals.DefaultErrorHandler>();
        }

        IServiceProvider provider = Services.BuildServiceProvider();

        CommandController controller = new(client, provider, _assemblies,
            _prefixes.ToArray(), _guildIds, _allowSlashCommands);
        controller.ApplicationFactory._conditions = _interactionConditions;

        return controller;
    }
}

using System.Reflection;
using DSharpPlus.UnifiedCommands.Message;
using DSharpPlus.UnifiedCommands.Message.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.UnifiedCommands;

public class UnifiedCommandsBuilder
{
    private Assembly? _assembly = null;
    private bool _isAlreadyBuilt = false;
    private readonly List<string> _prefixes = new();
    private ulong[]? _guildIds = null;
    private bool _allowSlashCommands = true;

    /// <summary>
    /// The service collection used for building the service provider.
    /// </summary>
    public IServiceCollection Services { get; internal set; } = new ServiceCollection();
    /// <summary>
    /// Configuration builder. It gets automatically added into dependency injection.
    /// </summary>
    public IConfigurationBuilder Configuration { get; internal set; } = new ConfigurationBuilder();


    /// <summary>
    /// Add the assembly used for reflection.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>Returns the builder.</returns>
    public UnifiedCommandsBuilder WithAssembly(Assembly assembly)
    {
        _assembly = assembly;
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

    /// <summary>
    /// Internal usage for building for the <see cref="CommandController">CommandController</see>
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal CommandController Build(DiscordClient client)
    {
        _isAlreadyBuilt = !_isAlreadyBuilt ? true : throw new Exception("The builder has already built an object.");

        if (_assembly is null)
        {
            throw new Exception("Please add a assembly before building.");
        }

        IConfiguration configuration = Configuration.Build();
        Services.AddSingleton(configuration);
        if (!Services.Any(s => s.ServiceType == typeof(IErrorHandler)))
        {
            client.Logger.LogTrace("Didn't find a error handler, using the default error handler");
            Services.AddSingleton<IErrorHandler>(new DefaultErrorHandler());
        }

        IServiceProvider provider = Services.BuildServiceProvider();

        CommandController controller = new(client, provider, _assembly,
            _prefixes.ToArray(), _guildIds, _allowSlashCommands);

        return controller;
    }
}

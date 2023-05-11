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
    private List<string> _prefixes = new();

    public IServiceCollection Services { get; internal set; } = new ServiceCollection();
    public IConfigurationBuilder Configuration { get; internal set; } = new ConfigurationBuilder();


    public UnifiedCommandsBuilder AddAssembly(Assembly assembly)
    {
        _assembly = assembly;
        return this;
    }

    public UnifiedCommandsBuilder AddPrefix(params string[] prefixes)
    {
        _prefixes.AddRange(prefixes);
        return this;
    }

    internal CommandController Build(DiscordClient client)
    {
        if (!_isAlreadyBuilt)
        {
            _isAlreadyBuilt = true;
        }
        else
        {
            throw new Exception("The builder has already built an object.");
        }

        if (_assembly is null)
        {
            throw new Exception("Please add a assembly before building.");
        }

        IConfiguration configuration = Configuration.Build();
        Services.AddSingleton(configuration);
        if (!Services.Any(s => s.ServiceType == typeof(IErrorHandler)))
        {
            client.Logger.LogInformation("Didn't find a error handler.");
            Services.AddSingleton<IErrorHandler>(new DefaultErrorHandler());
        }
        
        IServiceProvider provider = Services.BuildServiceProvider();

        CommandController controller = new(client, provider, _assembly,
            _prefixes.ToArray());

        return controller;
    }
}

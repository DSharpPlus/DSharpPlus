using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus.EventArgs;
using DSharpPlus.CH.Message.Internals;

namespace DSharpPlus.CH.Internals;

internal class CommandController
{
    private MessageCommandFactory _messageFactory;

    public MessageCommandFactory MessageCommandFactory
    {
        get => _messageFactory;
        private set => _messageFactory = value;
    }

    public CHConfiguration Configuration { get; set; }
    public ServiceProvider Services { get; set; }

    public CommandController(CHConfiguration configuration, DiscordClient client)
    {
        Configuration = configuration;


        if (Configuration.Services is null)
        {
            ServiceCollection? services = new();
            services.AddTransient<Message.IFailedConvertion, DefaultFailedConversion>();

            Services = services.BuildServiceProvider();
        }
        else
        {
            bool setDefaultIFailedConvertion = true;
            foreach (ServiceDescriptor? service in Configuration.Services)
            {
                if (service.ImplementationType?.IsSubclassOf(typeof(Message.IFailedConvertion)) ?? false)
                {
                    setDefaultIFailedConvertion = false;
                }
            }

            if (setDefaultIFailedConvertion)
            {
                Configuration.Services.AddTransient<Message.IFailedConvertion, DefaultFailedConversion>();
            }

            Services = Configuration.Services.BuildServiceProvider();
        }

        _messageFactory = new MessageCommandFactory { _services = Services, _configuration = configuration };

        CommandModuleRegister.RegisterMessageCommands(_messageFactory, Configuration.Assembly);
        client.MessageCreated += HandleMessageCreation;
    }

    public async Task HandleMessageCreation(DiscordClient client, MessageCreateEventArgs msg)
    {
        if (msg.Author.IsBot)
        {
            return;
        }

        if (Configuration.Prefix is null)
        {
            return;
        }

        if (!msg.Message.Content.StartsWith(Configuration.Prefix))
        {
            return;
        }

        string[]? content = msg.Message.Content.Remove(0, Configuration.Prefix.Count()).Split(' ');
        if (content.Count() == 0)
        {
            return;
        }

        string? command = content[0];
        string[]? args = content.Skip(1).ToArray(); // Command argument parsing needed here.
        await _messageFactory.ConstructAndExecuteCommandAsync(command, msg.Message, client, args);
    }
}

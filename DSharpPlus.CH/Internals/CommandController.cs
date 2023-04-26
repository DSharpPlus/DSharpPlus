using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.EventArgs;
using DSharpPlus.CH.Message.Internals;

namespace DSharpPlus.CH.Internals;

internal class CommandController
{
    public MessageCommandFactory MessageCommandFactory { get; private set; }

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

        MessageCommandFactory = new MessageCommandFactory { _services = Services, _configuration = configuration };

        CommandModuleRegister.RegisterMessageCommands(MessageCommandFactory, Configuration.Assembly);
        client.MessageCreated += HandleMessageCreationAsync;
    }

    public async Task HandleMessageCreationAsync(DiscordClient client, MessageCreateEventArgs msg)
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

        string[]? content = msg.Message.Content.Remove(0, Configuration.Prefix.Length).Split(' ');
        if (content.Length == 0)
        {
            return;
        }

        string? command = content[0];
        string[]? args = content.Skip(1).ToArray(); // Command argument parsing needed here.
        await MessageCommandFactory.ConstructAndExecuteCommandAsync(command, msg.Message, client, args);
    }
}

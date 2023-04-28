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

    public Task HandleMessageCreationAsync(DiscordClient client, MessageCreateEventArgs msg)
    {
        if (msg.Author.IsBot)
        {
            return Task.CompletedTask;
        }

        if (Configuration.Prefix is null)
        {
            return Task.CompletedTask;
        }

        if (!msg.Message.Content.StartsWith(Configuration.Prefix))
        {
            return Task.CompletedTask;
        }

        ReadOnlySpan<char> content = msg.Message.Content.AsSpan();
        if (content.Length == 0)
        {
            return Task.CompletedTask;
        }

        List<Range> ranges = new();
        Index last = 0;
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == ' ')
            {
                ranges.Add(new(last, i));
                last = i + 1;
            }
        }

        MessageCommandFactory.ConstructAndExecuteCommand(msg.Message, client, content, ranges);
        return Task.CompletedTask;
    }
}

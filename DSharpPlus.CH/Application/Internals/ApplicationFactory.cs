using DSharpPlus.CH.Application.Conditions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.CH.Application.Internals;

internal class ApplicationFactory
{
    private IServiceProvider _service;

    internal Dictionary<string, ApplicationMethodData> _methods = new(); // TODO: Change this into a tree.

    public ApplicationFactory(IServiceProvider service)
        => _service = service;

    internal void ExecuteCommand(DiscordInteraction interaction, DiscordClient client)
    {
        if (_methods.TryGetValue(interaction.Data.Name, out ApplicationMethodData? data))
        {
            object?[]? objects = MapParameters(data, interaction.Data.Options, interaction.Data.Resolved);
            ApplicationHandler handler = new(data, interaction,
                new List<Func<IServiceProvider, IApplicationCondition>>(), objects, _service.CreateScope(), client);

            _ = handler.BuildModuleAndExecuteCommandAsync();
        }
    }

    private object?[]? MapParameters(ApplicationMethodData data, IEnumerable<DiscordInteractionDataOption> options,
        DiscordInteractionResolvedCollection collection)
    {
        if (data.Parameters.Count == 0)
        {
            return null;
        }

        Dictionary<string, DiscordInteractionDataOption> dicOptions = new();
        foreach (DiscordInteractionDataOption option in options)
        {
            dicOptions.Add(option.Name, option);
        } // This is here to make ReSharp to shut up and to prevent looping over IEnumerable many times and only the need of doing it once.

        object?[] objects = new object[data.Parameters.Count];
        for (int i = 0; i < data.Parameters.Count; i++)
        {
            ApplicationMethodParameterData parameterData = data.Parameters[i];
            if (dicOptions.TryGetValue(parameterData.Name, out DiscordInteractionDataOption? option))
            {
                switch (parameterData.Type)
                {
                    case ApplicationCommandOptionType.Attachment:
                        objects[i] = collection.Attachments[(ulong)option.Value];
                        break;
                    case ApplicationCommandOptionType.Channel:
                        objects[i] = collection.Channels[(ulong)option.Value];
                        break;
                    case ApplicationCommandOptionType.User:
                        objects[i] = collection.Users[(ulong)option.Value];
                        break;
                    default:
                        objects[i] = option.Value;
                        break;
                }
            }
            else if (parameterData.IsNullable)
            {
                objects[i] = null;
            }
            else
            {
                throw new Exception("This is non existent for some reason.");
            }
        }

        return objects;
    }
}

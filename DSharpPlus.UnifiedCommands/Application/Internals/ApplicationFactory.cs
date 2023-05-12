using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands.Application.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Application.Internals;

internal class ApplicationFactory
{
    private IServiceProvider _service;
    private List<Func<IServiceProvider, IApplicationCondition>> _conditionBuilders = new();

    internal Dictionary<string, ApplicationMethodData> _methods = new(); // TODO: Change this into a tree.

    public ApplicationFactory(IServiceProvider service)
        => _service = service;

    internal void AddCondition(Func<IServiceProvider, IApplicationCondition> func)
        => _conditionBuilders.Add(func);
    
    internal void ExecuteCommand(DiscordInteraction interaction, DiscordClient client)
    {
        object?[]? objects;
        ApplicationHandler handler;
        
        if (_methods.TryGetValue(interaction.Data.Name, out ApplicationMethodData? data))
        {
            objects = MapParameters(data, interaction.Data.Options, interaction.Data.Resolved);
            handler = new(data, interaction,
                _conditionBuilders, objects, _service.CreateScope(), client);

            _ = handler.BuildModuleAndExecuteCommandAsync();
        }
        else
        {
            DiscordInteractionDataOption? subCommandOption = null;
            foreach (DiscordInteractionDataOption option in interaction.Data.Options)
            {
                if (option.Type == ApplicationCommandOptionType.SubCommand ||
                    option.Type == ApplicationCommandOptionType.SubCommandGroup)
                {
                    subCommandOption = option;
                    break;
                }
            }

            if (subCommandOption is not null)
            {
                if (subCommandOption.Type == ApplicationCommandOptionType.SubCommandGroup)
                {
                    DiscordInteractionDataOption? subCommandOption2 = null;
                    foreach (DiscordInteractionDataOption option in subCommandOption.Options)
                    {
                        if (option.Type == ApplicationCommandOptionType.SubCommand)
                        {
                            subCommandOption2 = option;
                            break;
                        }
                    }

                    if (subCommandOption2 is not null)
                    {
                        string name =
                            $"{interaction.Data.Name} {(string)subCommandOption.Name} {subCommandOption2.Name}";
                        if (_methods.TryGetValue(name, out ApplicationMethodData? subMethodData))
                        {
                            objects = MapParameters(subMethodData, subCommandOption.Options,
                                interaction.Data.Resolved);
                            handler = new(subMethodData, interaction,
                                _conditionBuilders, objects,
                                _service.CreateScope(),
                                client);

                            _ = handler.BuildModuleAndExecuteCommandAsync();
                        }
                    }
                }
                else
                {
                    string name = $"{interaction.Data.Name} {(string)subCommandOption.Name}";
                    if (_methods.TryGetValue(name, out ApplicationMethodData? methodData))
                    {
                        objects = MapParameters(methodData, subCommandOption.Options,
                            interaction.Data.Resolved);
                        handler = new(methodData, interaction,
                            _conditionBuilders, objects, _service.CreateScope(),
                            client);

                        _ = handler.BuildModuleAndExecuteCommandAsync();
                    }
                }
            }
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

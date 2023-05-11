using DSharpPlus.CH.Application.Conditions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Application.Internals;

internal class ApplicationFactory
{
    private IServiceProvider _service;

    internal Dictionary<string, ApplicationMethodData> _methods = new(); // TODO: Change this into a tree.

    public ApplicationFactory(IServiceProvider service)
        => _service = service;

    internal void ExecuteCommand(DiscordInteraction interaction)
    {
        if (_methods.TryGetValue(interaction.Data.Name, out ApplicationMethodData? data))
        {
            object?[]? objects = MapParameters(data, interaction.Data.Options);
            ApplicationHandler handler = new(data, interaction,
                new List<Func<IServiceProvider, IApplicationCondition>>(), objects, _service.CreateScope());
            
            _ = handler.BuildModuleAndExecuteCommandAsync();
        }
    }

    private object?[]? MapParameters(ApplicationMethodData data, IEnumerable<DiscordInteractionDataOption> options)
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
                objects[i] = option.Value;
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

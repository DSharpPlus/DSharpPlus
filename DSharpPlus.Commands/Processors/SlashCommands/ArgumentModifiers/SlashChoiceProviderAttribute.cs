using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashChoiceProviderAttribute(Type providerType) : Attribute
{
    public Type ProviderType { get; init; } = providerType ?? throw new ArgumentNullException(nameof(providerType));

    public async ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> GrabChoicesAsync(IServiceProvider serviceProvider, CommandParameter parameter)
    {
        IChoiceProvider choiceProvider;
        try
        {
            choiceProvider = (IChoiceProvider)ActivatorUtilities.CreateInstance(serviceProvider, this.ProviderType);
        }
        catch (Exception e)
        {
            ILogger<SlashCommandProcessor> logger = serviceProvider.GetRequiredService<ILogger<SlashCommandProcessor>>();
            logger.LogWarning("""ChoiceProvider for parameter "{ParameterName}" was not able to be constructed: {ExceptionType}""", parameter.Name, e.Message);
            return [];
        }

        List<DiscordApplicationCommandOptionChoice> choices = [];
        foreach ((string name, object value) in await choiceProvider.ProvideAsync(parameter))
        {
            choices.Add(new(name, value));
        }

        return choices;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class SlashChoiceProviderAttribute<T> : SlashChoiceProviderAttribute where T : IChoiceProvider
{
    public SlashChoiceProviderAttribute() : base(typeof(T)) { }
}

public interface IChoiceProvider
{
    public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashChoiceProviderAttribute : Attribute
{
    public Type ProviderType { get; init; }

    public SlashChoiceProviderAttribute(Type providerType)
    {
        ArgumentNullException.ThrowIfNull(providerType, nameof(providerType));
        if (providerType.GetInterface(nameof(IChoiceProvider)) is null)
        {
            throw new ArgumentException("The provided type must implement IChoiceProvider.", nameof(providerType));
        }

        this.ProviderType = providerType;
    }

    public async ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> GrabChoicesAsync(IServiceProvider serviceProvider, CommandParameter parameter)
    {
        IChoiceProvider choiceProvider;
        try
        {
            choiceProvider = (IChoiceProvider)
                ActivatorUtilities.CreateInstance(serviceProvider, this.ProviderType);
        }
        catch (Exception error)
        {
            ILogger<SlashCommandProcessor> logger = serviceProvider.GetRequiredService<ILogger<SlashCommandProcessor>>();
            logger.LogError(
                error,
                "ChoiceProvider '{Type}' for parameter '{ParameterName}' was not able to be constructed.",
                this.ProviderType,
                parameter.ToString()
            );

            return [];
        }

        List<DiscordApplicationCommandOptionChoice> choices = new(25);
        foreach (DiscordApplicationCommandOptionChoice choice in await choiceProvider.ProvideAsync(parameter))
        {
            if (choices.Count == 25)
            {
                ILogger<SlashCommandProcessor> logger = serviceProvider.GetRequiredService<ILogger<SlashCommandProcessor>>();
                logger.LogWarning(
                    "ChoiceProvider '{Type}' for parameter '{ParameterName}' returned more than 25 choices, only the first 25 will be used.",
                    this.ProviderType,
                    parameter.ToString()
                );

                break;
            }

            choices.Add(choice);
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
    public ValueTask<IEnumerable<DiscordApplicationCommandOptionChoice>> ProvideAsync(CommandParameter parameter);
}

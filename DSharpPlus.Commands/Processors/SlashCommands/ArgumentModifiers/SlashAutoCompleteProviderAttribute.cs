using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashAutoCompleteProviderAttribute : Attribute
{
    public Type AutoCompleteType { get; init; }

    public SlashAutoCompleteProviderAttribute(Type autoCompleteType)
    {
        ArgumentNullException.ThrowIfNull(autoCompleteType, nameof(autoCompleteType));
        this.AutoCompleteType = autoCompleteType;
    }

    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(
        AutoCompleteContext context
    )
    {
        IAutoCompleteProvider autoCompleteProvider;
        try
        {
            autoCompleteProvider = (IAutoCompleteProvider)
                ActivatorUtilities.CreateInstance(context.ServiceProvider, this.AutoCompleteType);
        }
        catch (Exception error)
        {
            ILogger<IAutoCompleteProvider> logger = context.ServiceProvider.GetRequiredService<
                ILogger<IAutoCompleteProvider>
            >();
            logger.LogError(
                error,
                "AutoCompleteProvider '{Type}' for parameter '{ParameterName}' was not able to be constructed.",
                this.AutoCompleteType,
                context.Parameter.ToString()
            );
            return [];
        }

        List<DiscordAutoCompleteChoice> choices = new(25);
        foreach (
            DiscordAutoCompleteChoice choice in await autoCompleteProvider.AutoCompleteAsync(
                context
            )
        )
        {
            if (choices.Count == 25)
            {
                ILogger<IAutoCompleteProvider> logger = context.ServiceProvider.GetRequiredService<
                    ILogger<IAutoCompleteProvider>
                >();
                logger.LogWarning(
                    "AutoCompleteProvider '{Type}' for parameter '{ParameterName}' returned more than 25 choices, only the first 25 will be used.",
                    this.AutoCompleteType,
                    context.Parameter.ToString()
                );
                break;
            }

            choices.Add(choice);
        }

        return choices;
    }
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter,
    Inherited = false,
    AllowMultiple = false
)]
public sealed class SlashAutoCompleteProviderAttribute<T> : SlashAutoCompleteProviderAttribute
    where T : IAutoCompleteProvider
{
    public SlashAutoCompleteProviderAttribute()
        : base(typeof(T)) { }
}

public interface IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(
        AutoCompleteContext context
    );
}

namespace DSharpPlus.Commands.Processors.SlashCommands.Modifiers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashAutoCompleteProviderAttribute(Type autoCompleteType) : Attribute
{
    public Type AutoCompleteType { get; init; } = autoCompleteType ?? throw new ArgumentNullException(nameof(autoCompleteType));

    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        IAutoCompleteProvider autoCompleteProvider;
        try
        {
            autoCompleteProvider = (IAutoCompleteProvider)ActivatorUtilities.CreateInstance(context.ServiceProvider, this.AutoCompleteType);
        }
        catch (Exception)
        {
            return [];
        }

        List<DiscordAutoCompleteChoice> choices = [];
        foreach ((string name, object value) in await autoCompleteProvider.AutoCompleteAsync(context))
        {
            choices.Add(new(name, value));
        }

        return choices;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class SlashAutoCompleteProviderAttribute<T> : SlashAutoCompleteProviderAttribute where T : IAutoCompleteProvider
{
    public SlashAutoCompleteProviderAttribute() : base(typeof(T)) { }
}

public interface IAutoCompleteProvider
{
    public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext context);
}

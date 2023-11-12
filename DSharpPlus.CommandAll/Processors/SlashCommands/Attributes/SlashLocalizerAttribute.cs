namespace DSharpPlus.CommandAll.Processors.SlashCommands.Attributes;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Processors.SlashCommands.Translation;
using Microsoft.Extensions.DependencyInjection;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashLocalizerAttribute(Type localizerType) : Attribute
{
    public Type LocalizerType { get; init; } = localizerType ?? throw new ArgumentNullException(nameof(localizerType));

    public async Task<IReadOnlyDictionary<string, string>> LocalizeAsync(IServiceProvider serviceProvider, string fullSymbolName)
    {
        ITranslator translator;
        try
        {
            translator = (ITranslator)ActivatorUtilities.CreateInstance(serviceProvider, this.LocalizerType);
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }

        Dictionary<string, string> localized = [];
        foreach ((Locales locale, string translation) in await translator.TranslateAsync(fullSymbolName.Replace(' ', '.').ToLowerInvariant()))
        {
            localized.Add(locale.ToString().Replace('_', '-'), translation);
        }

        return localized;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class SlashLocalizerAttribute<T> : SlashLocalizerAttribute where T : ITranslator
{
    public SlashLocalizerAttribute() : base(typeof(T)) { }
}

public interface ITranslator
{
    public Task<IReadOnlyDictionary<Locales, string>> TranslateAsync(string fullSymbolName);
}

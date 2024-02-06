namespace DSharpPlus.Commands.Processors.SlashCommands.Attributes;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class SlashLocalizerAttribute(Type localizerType) : Attribute
{
    public Type LocalizerType { get; init; } = localizerType ?? throw new ArgumentNullException(nameof(localizerType));

    public async Task<Dictionary<string, string>> LocalizeAsync(IServiceProvider serviceProvider, string fullSymbolName)
    {
        ITranslator translator;
        try
        {
            translator = (ITranslator)ActivatorUtilities.CreateInstance(serviceProvider, this.LocalizerType);
        }
        catch (Exception)
        {
            ILogger<SlashLocalizerAttribute> logger = serviceProvider.GetService<ILogger<SlashLocalizerAttribute>>() ?? NullLogger<SlashLocalizerAttribute>.Instance;
            logger.LogWarning("Failed to create an instance of {TypeName} for localization of {SymbolName}.", this.LocalizerType, fullSymbolName);
            return [];
        }

        Dictionary<string, string> localized = [];
        foreach ((DiscordLocale locale, string translation) in await translator.TranslateAsync(fullSymbolName.Replace(' ', '.').ToLowerInvariant()))
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
    public Task<IReadOnlyDictionary<DiscordLocale, string>> TranslateAsync(string fullSymbolName);
}

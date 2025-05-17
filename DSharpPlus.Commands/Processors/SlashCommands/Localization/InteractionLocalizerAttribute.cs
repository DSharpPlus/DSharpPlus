using System;

namespace DSharpPlus.Commands.Processors.SlashCommands.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public class InteractionLocalizerAttribute(Type localizerType) : Attribute
{
    public Type LocalizerType { get; init; } = localizerType ?? throw new ArgumentNullException(nameof(localizerType));
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class InteractionLocalizerAttribute<T> : InteractionLocalizerAttribute where T : IInteractionLocalizer
{
    public InteractionLocalizerAttribute() : base(typeof(T)) { }
}

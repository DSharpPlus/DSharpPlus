namespace DSharpPlus.CommandsNext.Converters;

using System;
using Microsoft.Extensions.DependencyInjection;

internal class HelpFormatterFactory
{
    private ObjectFactory Factory { get; set; } = null!;

    public HelpFormatterFactory() { }

    public void SetFormatterType<T>() where T : BaseHelpFormatter => Factory = ActivatorUtilities.CreateFactory(typeof(T), [typeof(CommandContext)]);

    public BaseHelpFormatter Create(CommandContext ctx)
        => Factory is null
            ? throw new InvalidOperationException($"A formatter type must be set with the {nameof(this.SetFormatterType)} method.")
            : (BaseHelpFormatter)Factory(ctx.Services, [ctx]);
}

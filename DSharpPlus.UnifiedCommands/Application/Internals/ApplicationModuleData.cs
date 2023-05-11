using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Application.Internals;

internal class ApplicationModuleData
{
    public ObjectFactory Factory { get; }

    public ApplicationModuleData(ObjectFactory factory)
        => Factory = factory;
}

using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageModuleData
{
    public ObjectFactory Factory { get; set; } = null!;
}

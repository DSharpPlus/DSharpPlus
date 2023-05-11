using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageModuleData
{
    public ObjectFactory Factory { get; set; } = null!;
}

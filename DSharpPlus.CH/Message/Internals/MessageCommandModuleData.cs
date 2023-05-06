using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CH.Message.Internals;

internal class MessageCommandModuleData
{
    public ObjectFactory Factory { get; set; } = null!;
}

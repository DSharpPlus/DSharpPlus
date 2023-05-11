using System.Reflection;

namespace DSharpPlus.UnifiedCommands.Application.Internals;

internal class ApplicationMethodData
{
    public required bool IsAsync { get; set; }
    public required bool ReturnsNothing { get; set; }
    public required MethodInfo Method { get; set; }
    public required ApplicationModuleData Module { get; set; }
    public List<ApplicationMethodParameterData> Parameters { get; set; } = new();
}

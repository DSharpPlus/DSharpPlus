using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DSharpPlus.CH;

public class CHConfiguration
{
    public required Assembly Assembly { get; set; }
    public ServiceCollection? Services { get; set; }
    internal List<Type> Conditions { get; set; } = new();
    public string? Prefix { get; set; }

    public CHConfiguration AddMessageCondition<T>() where T : Message.IMessageCondition
    {
        Conditions.Add(typeof(T));
        return this;
    }
}

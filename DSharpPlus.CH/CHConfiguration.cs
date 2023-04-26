using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DSharpPlus.CH
{

    public class CHConfiguration
    {
        public required Assembly Assembly { get; set; }
        public ServiceCollection? Services { get; set; }
        internal List<Type> Middlewares { get; set; } = new List<Type>();
        public string? Prefix { get; set; }

        public CHConfiguration AddMessageMiddleware<T>() where T : Message.IMessageMiddleware
        {
            Middlewares.Add(typeof(T));
            return this;
        }
    }
}

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DSharpPlus.CH
{

    public class CHConfiguration
    {
        public required Assembly Assembly { get; set; }
        public ServiceCollection? Services { get; set; }
        public List<Type> Middlewares { get; set; } = new List<Type>();
        public string? Prefix { get; set; }

        public CHConfiguration AddMiddleware<T>()
        {
            Middlewares.Add(typeof(T));
            return this;
        }
    }
}

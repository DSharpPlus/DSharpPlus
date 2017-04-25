using System.Linq;
using System.Reflection;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace System
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        internal AppDomain()
        {
        }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public Assembly[] GetAssemblies()
        {
            var rid = RuntimeEnvironment.GetRuntimeIdentifier();
            var ass = DependencyContext.Default.GetRuntimeAssemblyNames(rid);

            return ass.Select(xan => Assembly.Load(xan)).ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.CommandsNext
{
    internal struct DependencyInfo
    {
        public Type ImplementationType { get; internal set; }
        public Type DependencyType { get; internal set; }
        public object Instance { get; internal set; }
    }
}

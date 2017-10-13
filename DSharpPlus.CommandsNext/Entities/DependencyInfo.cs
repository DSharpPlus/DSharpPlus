using System;

// ReSharper disable once CheckNamespace
namespace DSharpPlus.CommandsNext
{
    internal struct DependencyInfo
    {
        public Type ImplementationType { get; internal set; }
        public Type DependencyType { get; internal set; }
        public object Instance { get; internal set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpPlus.CommandsNext
{
    public sealed class CommandParameter
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public bool IsOptional { get; private set; }
        public object DefaultValue { get; private set; }
    }
}

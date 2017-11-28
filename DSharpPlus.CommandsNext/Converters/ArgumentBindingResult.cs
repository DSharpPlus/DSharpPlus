using System.Collections.Generic;

namespace DSharpPlus.CommandsNext.Converters
{
    public struct ArgumentBindingResult
    {
        public object[] Converted { get; }
        public IReadOnlyList<string> Raw { get; }

        public ArgumentBindingResult(object[] converted, IReadOnlyList<string> raw)
        {
            this.Converted = converted;
            this.Raw = raw;
        }
    }
}

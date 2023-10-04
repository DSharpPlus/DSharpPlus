using System;
using System.Collections.Generic;

namespace DSharpPlus.CommandsNext.Converters;

public struct ArgumentBindingResult
{
    public bool IsSuccessful { get; }
    public object?[] Converted { get; }
    public IReadOnlyList<string> Raw { get; }
    public Exception? Reason { get; }

    public ArgumentBindingResult(object?[] converted, IReadOnlyList<string> raw)
    {
        this.IsSuccessful = true;
        this.Reason = null;
        this.Converted = converted;
        this.Raw = raw;
    }

    public ArgumentBindingResult(Exception ex)
    {
        this.IsSuccessful = false;
        this.Reason = ex;
        this.Converted = Array.Empty<object>();
        this.Raw = Array.Empty<string>();
    }
}

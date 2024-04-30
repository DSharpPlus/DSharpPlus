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
        IsSuccessful = true;
        Reason = null;
        Converted = converted;
        Raw = raw;
    }

    public ArgumentBindingResult(Exception ex)
    {
        IsSuccessful = false;
        Reason = ex;
        Converted = Array.Empty<object>();
        Raw = Array.Empty<string>();
    }
}
